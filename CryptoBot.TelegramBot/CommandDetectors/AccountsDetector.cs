using System.Text;
using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class AccountsDetector : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    
    public AccountsDetector(IStateFactory stateFactory, TelegramBot telegramBot, IServiceScopeFactory serviceScopeFactory)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public CommandDescription CommandDescription { get; } =
        new() { Command = "/accounts", Description = "Подключенные аккаунты" };

    public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
    {
        var receivedTelegramMessage = receivedUpdate.Message;
        var chatId = receivedUpdate.GetChatId();

        if (receivedTelegramMessage == null)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы сделали все что угодно, но не отправили мне комманду!",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());
            
            return null;
        }

        var text = receivedTelegramMessage.Text;

        if (text != CommandDescription.Command)
        {
            return null;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

        var accounts = await dbContext.Accounts
            .Where(x => x.ChatId == receivedTelegramMessage.Chat.Id)
            .Include(accountEntity => accountEntity.Exchange)
            .ToListAsync();

        var replyText = new StringBuilder("Подключенные аккаунты:\n");

        for (var i = 0; i < accounts.Count; i++)
        {
            replyText.Append($"{i + 1}. {accounts[i].Exchange.Exchange} - {accounts[i].Id}\n");
        }
        
        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId,
            replyText.ToString(),
            replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}