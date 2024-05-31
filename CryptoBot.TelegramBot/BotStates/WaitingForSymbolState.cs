using CryptoBot.Data.Entities;
using CryptoBot.Exchanges.Exchanges.Clients;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSymbolState : IBotState
{
    private readonly BybitApiClient _bybitApiClient;
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    
    public WaitingForSymbolState(BybitApiClient bybitApiClient, IStateFactory stateFactory, ILogger<WaitingForSymbolState> logger, TelegramBot telegramBot)
    {
        _bybitApiClient = bybitApiClient;
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
            await _telegramBot.SendDefaultMessageAsync("Некорректный ввод, попробуйте снова.", chatId);
            return this;
        }

        try
        {
            var result = await _bybitApiClient.GetLastTradedPrice(message);
            await _telegramBot.SendDefaultMessageAsync($"Последняя цена - {result}", chatId);
            return _stateFactory.CreateState(BotState.WaitingForCommand);
        }
        catch (InvalidOperationException ex)
        {
            await _telegramBot.SendDefaultMessageAsync($"Выбранная вами криптовалютная пара {message} не поддерживается", chatId);
            return this;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong {ex.Message}");

            await _telegramBot.SendDefaultMessageAsync("Внутренняя ошибка, попробуйте позже еще раз!", chatId);
            return this;
        }
    }
}