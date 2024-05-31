using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
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

    public Task StartReceivingMessagesAsync()
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = []
        };

        try
        {
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

        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var chatId = update.GetChatId();
        IBotState newBotState;
        IBotState currentBotState;

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var botDbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();
            var stateFactory = scope.ServiceProvider.GetRequiredService<IStateFactory>();


            var isChatNotExist = update?.Message?.Text == "/start" &&
                                 await botDbContext.Chats.AllAsync(x => x.ChatId != chatId,
                                     cancellationToken: cancellationToken);

            if (isChatNotExist)
            {
                await botDbContext.AddAsync(new ChatEntity
                {
                    BotState = BotState.WaitingForCommand,
                    Accounts = [],
                    ChatId = chatId
                }, cancellationToken);

                await botDbContext.SaveChangesAsync(cancellationToken);
            }

            var chat = await botDbContext.Chats.FirstOrDefaultAsync(x => x.ChatId == chatId,
                cancellationToken: cancellationToken);

            if (chat == null)
            {
                await SendDefaultMessageAsync("Для начала работы введите команду /start", chatId);
                return;
            }

            currentBotState = stateFactory.CreateState(chat.BotState);

            try
            {
                if (update.Message != null)
                {
                    _logger.LogInformation($"Received message from chat: {update.Message.Chat.Id}");
                }

                if (update.CallbackQuery is { Message: not null })
                {
                    _logger.LogInformation(
                        $"Received callback query from chat: {update.CallbackQuery.Message.Chat.Id}");
                }
            }
            catch (Exception)
            {
                // ignored
            }

            newBotState = await currentBotState.HandleUpdateAsync(update);
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var botDbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var chat = await botDbContext.Chats.FirstOrDefaultAsync(x => x.ChatId == chatId,
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

    public async Task SendDefaultMessageAsync(string text, long chatId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        await botClient.SendTextMessageAsync(chatId, text);
    }
}
