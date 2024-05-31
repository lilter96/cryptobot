using CryptoBot.TelegramBot.BotStates;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.Classes;

public class TelegramBot
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBot> _logger;
    private readonly CommandDetectorService _commandDetectorService;
    private readonly List<BotCommand> LastBotsCommands = [];

    private IBotState _currentBotState;

    public TelegramBot(ITelegramBotClient botClient, ILogger<TelegramBot> logger, CommandDetectorService commandDetectorService)
    {
        _botClient = botClient;
        _logger = logger;
        _commandDetectorService = commandDetectorService;
    }

    public Task StartReceivingMessagesAsync()
    {
        _currentBotState = new WaitingForCommandState(_commandDetectorService);
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

        var newBotState = await _currentBotState.HandleUpdateAsync(update, this);

        _currentBotState = newBotState ?? _currentBotState;

        if (newBotState?.Command != null)
        {
            LastBotsCommands.Add(newBotState.Command.Value);
        }
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

    public async Task SendDefaultMessageAsync(string text, long chatId)
    {
        await _botClient.SendTextMessageAsync(chatId, text);
    }
}
