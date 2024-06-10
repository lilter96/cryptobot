using System.ComponentModel.DataAnnotations;
using CryptoBot.Service.Services.Chat;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot;

public class TelegramBot
{
    public ITelegramBotClient BotClient { get; }
    private readonly ILogger<TelegramBot> _logger;
    private readonly IChatService _chatService;
    private readonly IStateFactory _stateFactory;
    private readonly CommandDetectorService _commandDetectorService;

    public TelegramBot(ITelegramBotClient botClient, ILogger<TelegramBot> logger, IChatService chatService, IStateFactory stateFactory, CommandDetectorService commandDetectorService)
    {
        BotClient = botClient;
        _logger = logger;
        _chatService = chatService;
        _stateFactory = stateFactory;
        _commandDetectorService = commandDetectorService;
    }

    public async Task StartReceivingMessagesAsync()
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = []
        };

        try
        {
            await BotClient.DeleteMyCommandsAsync(BotCommandScope.AllPrivateChats());
            await SetDefaultCommandsAsync();

            BotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions);

            _logger.LogInformation("Bot successfully launched.");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        await Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var chatId = update.GetChatId();
        IBotState newBotState = null;

        var currentBotState = _stateFactory.CreateState((BotState) await _chatService.GetBotStateAsync(chatId));

        try
        {
            newBotState = await currentBotState.HandleUpdateAsync(update);
        }
        catch (ValidationException)
        {
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Введена не верная команда",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard(),
                cancellationToken: cancellationToken);
        }

        var patchDocument = new JsonPatchDocument();

        currentBotState = newBotState ?? currentBotState;

        patchDocument.Replace("/BotState", (int)currentBotState.BotState);

        await _chatService.PatchChatAsync(chatId, patchDocument);
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

    private async Task SetDefaultCommandsAsync()
    {
        // Only lower case without symblos
        var commands = _commandDetectorService
            .AllCommands
            .Select(x =>
                new BotCommand
                {
                    Command = x.Command,
                    Description = x.Description
                });

        await BotClient.SetMyCommandsAsync(commands, BotCommandScope.AllPrivateChats());
    }
}
