using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.String;

namespace CryptoBot.TelegramBot.BotStates;

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

        if (IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        var isExchange = Enum.TryParse<Exchange>(message, true, out var exchange);

        if (!isExchange)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы выбрали не поддерживаемую биржу!",
                replyMarkup: TelegramKeyboards.GetExchangeSelectingKeyboard(true));

            return await Task.FromResult((IBotState) null);
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var chat = await dbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId);

            var newAccountId = Guid.NewGuid();

            var newAccount = new AccountEntity
            {
                Id = newAccountId,
                ChatId = chat.Id,
                Exchange = new ExchangeEntity
                {
                    Exchange = exchange,
                    AccountId = newAccountId
                }
            };

            await dbContext.Accounts.AddAsync(newAccount);

            chat.SelectedAccountId = newAccount.Id;
            dbContext.Update(chat);

            await dbContext.SaveChangesAsync();

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Введите секретный API ключ! ВНИМАНИЕ: используйте ключ только для чтения",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            return _stateFactory.CreateState(BotState.WaitingForExchangeApiKeyState);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
