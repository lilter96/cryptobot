using CryptoBot.Service.Services.ExchangeApi;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSymbolState : IBotState
{
    private readonly IExchangeApiService _exchangeApiService;
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;

    public WaitingForSymbolState(
        IExchangeApiService exchangeApiService,
        IStateFactory stateFactory,
        ILogger<WaitingForSymbolState> logger,
        TelegramBot telegramBot)
    {
        _exchangeApiService = exchangeApiService;
        _stateFactory = stateFactory;
        _logger = logger;
        _telegramBot = telegramBot;
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
            var result = await _exchangeApiService.GetSymbolPrice(chatId, message);

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
