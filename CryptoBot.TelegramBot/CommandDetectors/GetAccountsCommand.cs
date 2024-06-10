using System.Text;
using CryptoBot.Service.Services.Account;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class GetAccountsCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAccountService _accountService;

    public GetAccountsCommand(
        IStateFactory stateFactory,
        TelegramBot telegramBot,
        IServiceScopeFactory serviceScopeFactory,
        IAccountService accountService)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
        _accountService = accountService;
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


        var accounts = await _accountService.GetAccountsAsync(chatId, 10);

        var replyText = new StringBuilder("Подключенные аккаунты:\n");

        for (var i = 0; i < accounts.Count; i++)
        {
            replyText.Append($"{i + 1}. {accounts[i].Exchange} - {accounts[i].Id}\n");
        }

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId,
            replyText.ToString(),
            replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}
