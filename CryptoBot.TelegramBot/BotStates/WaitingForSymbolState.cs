using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.Service.Services.Interfaces;
using CryptoBot.TelegramBot.Keyboards;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSymbolState : IBotState
{
    private readonly BybitApiClient _bybitApiClient;
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICryptoService _cryptoService;

    public WaitingForSymbolState(BybitApiClient bybitApiClient, IStateFactory stateFactory, ILogger<WaitingForSymbolState> logger, TelegramBot telegramBot, IServiceScopeFactory serviceScopeFactory, ICryptoService cryptoService)
    {
        _bybitApiClient = bybitApiClient;
        _stateFactory = stateFactory;
        _logger = logger;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
        _cryptoService = cryptoService;
    }

    public BotState BotState { get; set; } = BotState.WaitingForSymbol;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        var message = update.Message?.Text ?? string.Empty;

        var chatId = update.GetChatId();

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());
            
            return this;
        }

        try
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var chat = await dbContext.Chats.Include(chatEntity => chatEntity.SelectedAccount)
                .ThenInclude(accountEntity => accountEntity.Exchange).FirstOrDefaultAsync(x => x.Id == chatId);

            if (chat.SelectedAccountId == null)
            {
                await _telegramBot.BotClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Не возможно просмотреть цену, вы не подключили ни одного аккаунта, либо не выбрали аккаунт",
                    replyMarkup: TelegramKeyboards.GetDefaultKeyboard());
                
                return _stateFactory.CreateState(BotState.WaitingForCommand);
            }

            var decryptedKey = await _cryptoService.DecryptAsync(chat.SelectedAccount.Exchange.EncryptedKey);
            var decryptedSecret = await _cryptoService.DecryptAsync(chat.SelectedAccount.Exchange.EncryptedSecret);

            var apiCredentials = new ApiCredentials(decryptedKey, decryptedSecret);

            var result = await _bybitApiClient.GetLastTradedPriceAsync(apiCredentials, message);
            
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Последняя цена - {result}",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());
            
            return _stateFactory.CreateState(BotState.WaitingForCommand);
        }
        catch (InvalidOperationException)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Выбранная вами криптовалютная пара {message} не поддерживается",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());
            
            return this;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong {ex.Message}");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Внутренняя ошибка, попробуйте позже еще раз!",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }
    }
}
