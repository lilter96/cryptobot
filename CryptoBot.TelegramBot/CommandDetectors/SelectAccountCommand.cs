using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class SelectAccountCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SelectAccountCommand(IStateFactory stateFactory, TelegramBot telegramBot, IServiceScopeFactory serviceScopeFactory)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public CommandDescription CommandDescription { get; } =
        new() { Command = "/selectaccount", Description = "Выбрать рабочий аккаунт" };

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

        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

        var accounts = await dbContext.Accounts
            .Where(x => x.ChatId == receivedTelegramMessage.Chat.Id)
            .Include(accountEntity => accountEntity.Exchange)
            .ToListAsync();

        if (text == CommandDescription.Command)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Подключенные аккаунты",
                replyMarkup: TelegramKeyboards.GetSelectingAccountInlineKeyboard(accounts));

            return _stateFactory.CreateState(BotState.WaitingForSelectingAccount);
        }

        return null;
    }
}
