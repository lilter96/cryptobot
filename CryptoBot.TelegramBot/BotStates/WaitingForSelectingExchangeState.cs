using CryptoBot.Data;
using CryptoBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates
{
    public class WaitingForSelectingExchangeState : IBotState
    {
        private readonly IStateFactory _stateFactory;
        private readonly ILogger<WaitingForSymbolState> _logger;
        private readonly TelegramBot _telegramBot;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WaitingForSelectingExchangeState(
            IStateFactory stateFactory,
            ILogger<WaitingForSymbolState> logger,
            IServiceScopeFactory serviceScopeFactory,
            TelegramBot telegramBot)
        {
            _stateFactory = stateFactory;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _telegramBot = telegramBot;
        }

        public BotState BotState { get; set; } = BotState.WaitingForSelectingExchange;

        public async Task<IBotState> HandleUpdateAsync(Update update)
        {
            var message = update?.Message?.Text;

            var chatId = update.GetChatId();

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Empty message in update from Telegram");
                await _telegramBot.SendDefaultMessageAsync("Некорректный ввод, попробуйте снова.", chatId);
                return this;
            }

            var isExchange = Enum.TryParse<Exchange>(message, true, out var exchange);

            if (!isExchange)
            {
                await _telegramBot.SendDefaultMessageAsync("Вы выбрали не поддерживаемую биржу!", chatId);
                return await Task.FromResult((IBotState) null);
            }

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

                var chat = await dbContext.Chats.FirstOrDefaultAsync(x => x.ChatId == chatId);

                AccountEntity editedAccount;
                if (chat.SelectedAccountId == null)
                {
                    var newAccountId = Guid.NewGuid();

                    editedAccount = new AccountEntity
                    {
                        Id = newAccountId,
                        ChatId = chat.Id,
                        Exchange = new ExchangeEntity
                        {
                            Exchange = exchange,
                            AccountId = newAccountId
                        }
                    };

                    await dbContext.Accounts.AddAsync(editedAccount);

                    chat.SelectedAccountId = editedAccount.Id;
                    dbContext.Update(chat);
                }
                else
                {
                    editedAccount = await dbContext.Accounts
                        .Include(accountEntity => accountEntity.Exchange)
                        .FirstOrDefaultAsync(x => x.Id == chat.SelectedAccountId);

                    editedAccount.Exchange.Exchange = exchange;

                    dbContext.Update(editedAccount);
                }

                await dbContext.SaveChangesAsync();

                await _telegramBot.SendDefaultMessageAsync(
                    "Введите секретный API ключ! ВНИМАНИЕ: используйте ключ только для чтения", chatId);

                return _stateFactory.CreateState(BotState.WaitingForExchangeApiKeyState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
