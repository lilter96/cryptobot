using CryptoBot.Service.Services.Account;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class SelectAccountCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IAccountService _accountService;

    public SelectAccountCommand(
        IStateFactory stateFactory,
        TelegramBot telegramBot,
        IAccountService accountService)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _accountService = accountService;
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

        var accounts = await _accountService.GetAccountsAsync(chatId, 10);

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
