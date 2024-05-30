using System.Globalization;
using CryptoBot.Exchanges.Exchanges.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.Classes;

public class TelegramBot
{
    private readonly ITelegramBotClient _botClient;
    private readonly BybitApiClient _bybitApiClient;
    private readonly ILogger<TelegramBot> _logger;

    public TelegramBot(ITelegramBotClient botClient, ILogger<TelegramBot> logger, BybitApiClient bybitApiClient)
    {
        _botClient = botClient;
        _logger = logger;
        _bybitApiClient = bybitApiClient;
    }

    public Task StartReceivingMessagesAsync()
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = []
        };

        try
        {
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions);

            _logger.LogInformation("Bot successfully launched.");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Task.CompletedTask;
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null)
            {
                _logger.LogInformation($"Received message from chat: {update.Message.Chat.Id}");
            }

            if (update.CallbackQuery is { Message: not null })
            {
                _logger.LogInformation($"Received callback query from chat: {update.CallbackQuery.Message.Chat.Id}");
            }
        }
        catch (Exception)
        {
            // ignored
        }

        var message = string.Empty;
        
        switch (update.Message?.Text)
        {
            case "/start":
                message = "Hello!";
                break;
            case "BTCUSDT":
                var response = await _bybitApiClient.GetLastTradedPrice(update.Message.Text.ToUpper());
                message = response.ToString(CultureInfo.InvariantCulture);
                break;
            default:
                message = "aboba";
                break;
        }
        
        await botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: message,
            cancellationToken: cancellationToken);
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        _ = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        return Task.CompletedTask;
    }
}