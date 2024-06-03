using System.ComponentModel.DataAnnotations;
using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.CommandDetectors;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TelegramBot(ITelegramBotClient botClient, ILogger<TelegramBot> logger, IServiceScopeFactory serviceScopeFactory)
    {
        BotClient = botClient;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
        IBotState currentBotState;

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var botDbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();
            var stateFactory = scope.ServiceProvider.GetRequiredService<IStateFactory>();

            var chat = await botDbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId,
                cancellationToken: cancellationToken);

            if (update.Message != null && chat == null && update.Message.Text != "/start")
            {
                await BotClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Для начала работы введите команду /start",
                    replyMarkup: TelegramKeyboards.GetEmptyKeyboard(),
                    cancellationToken: cancellationToken);
                
                return;
            }
            
            currentBotState = stateFactory.CreateState(
                update.Message is { Text: "/start" } 
                ? BotState.WaitingForCommand 
                : chat.BotState);

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
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var botDbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var chat = await botDbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId,
                cancellationToken: cancellationToken);

            currentBotState = newBotState ?? currentBotState;

            chat.BotState = currentBotState.BotState;

            botDbContext.Chats.Update(chat);
            await botDbContext.SaveChangesAsync(cancellationToken);
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
    
    private async Task SetDefaultCommandsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var commandDetectorService = scope.ServiceProvider.GetRequiredService<CommandDetectorService>();

        // Only lower case without symblos
        var commands = commandDetectorService
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
