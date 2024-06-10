using CryptoBot.Service.Services.Account;
using CryptoBot.Service.Services.ExchangeApi;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class CreateAccountCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IAccountService _accountService;
    private readonly IExchangeApiService _exchangeApiService;

    public CreateAccountCommand(
        IStateFactory stateFactory,
        TelegramBot telegramBot,
        IAccountService accountService,
        IExchangeApiService exchangeApiService)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _accountService = accountService;
        _exchangeApiService = exchangeApiService;
    }

    public CommandDescription CommandDescription { get; } =
        new() { Command = "/addaccount", Description = "Добавить аккаунт биржи" };

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

        await _accountService.CreateAccountAndSelectItAsync(chatId);

        var keyboard = TelegramKeyboards.GetExchangeSelectingKeyboard(_exchangeApiService.GetExchangeNames());

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId,
            "Выберите биржу из списка!",
            replyMarkup: keyboard);

        return _stateFactory.CreateState(BotState.WaitingForSelectingExchange);
    }
}
