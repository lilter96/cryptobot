using System.ComponentModel.DataAnnotations;
using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class StartCommandDetector : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public StartCommandDetector(IStateFactory stateFactory, TelegramBot telegramBot, IServiceScopeFactory serviceScopeFactory)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public CommandDescription CommandDescription { get; } =
        new() { Command = "/start", Description = "Запустить бота" };

    public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
    {
        var receivedTelegramMessage = receivedUpdate.Message;
        var chatId = receivedUpdate.GetChatId();

        var text = receivedTelegramMessage.Text;
        
        if (text != CommandDescription.Command)
        {
            throw new ValidationException();
        }
        
        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();
        
        var isChatNotExist = receivedTelegramMessage is { Text: "/start" } &&
                             await dbContext.Chats.AllAsync(x => x.Id != chatId);

        if (isChatNotExist)
        {
            dbContext.Add(new ChatEntity
            {
                BotState = BotState.WaitingForCommand,
                Accounts = [],
                Id = chatId
            });

            await dbContext.SaveChangesAsync();
        }
        
        var keyboard = TelegramKeyboards.GetDefaultKeyboard(true);
        
        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId,
            "Доброго времени суток, с вами БОТ КРИПТОПАМПИКС!",
            replyMarkup: keyboard);
        
        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}
