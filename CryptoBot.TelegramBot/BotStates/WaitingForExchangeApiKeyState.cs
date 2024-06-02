using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates
{
    public class WaitingForExchangeApiKeyState : IBotState
    {
        private readonly ILogger<WaitingForSymbolState> _logger;
        private readonly TelegramBot _telegramBot;
        private readonly ICryptoService _cryptoService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IStateFactory _stateFactory;

        public WaitingForExchangeApiKeyState(
            ILogger<WaitingForSymbolState> logger,
            TelegramBot telegramBot,
            ICryptoService cryptoService,
            IServiceScopeFactory serviceScopeFactory,
            IStateFactory stateFactory)
        {
            _logger = logger;
            _telegramBot = telegramBot;
            _cryptoService = cryptoService;
            _serviceScopeFactory = serviceScopeFactory;
            _stateFactory = stateFactory;
        }

        public BotState BotState { get; set; } = BotState.WaitingForExchangeApiKeyState;

        public async Task<IBotState> HandleUpdateAsync(Update update)
        {
            var apiKey = update?.Message?.Text;

            var chatId = update.GetChatId();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Empty message in update from Telegram");
                await _telegramBot.SendDefaultMessageAsync("Некорректный ввод, попробуйте снова.", chatId);
                return this;
            }

            var encryptedApiKey = await _cryptoService.EncryptAsync(apiKey);

            using var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var chat = await dbContext.Chats
                .Include(chatEntity => chatEntity.SelectedAccount)
                .ThenInclude(x => x.Exchange)
                .FirstOrDefaultAsync(x => x.Id == chatId);

            chat.SelectedAccount.Exchange.EncryptedKey = encryptedApiKey;

            await dbContext.SaveChangesAsync();

            await _telegramBot.BotClient.DeleteMessageAsync(chatId, update.Message.MessageId);
            await _telegramBot.SendDefaultMessageAsync("Сообщение с API ключом удалено в целях вашей безопасности!", chatId);

            return _stateFactory.CreateState(BotState.WaitingForExchangeApiSecretState);
        }
    }
}
