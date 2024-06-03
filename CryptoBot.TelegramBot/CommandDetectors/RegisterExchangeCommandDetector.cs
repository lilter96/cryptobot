using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class RegisterExchangeCommandDetector : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    
    public RegisterExchangeCommandDetector(IStateFactory stateFactory, TelegramBot telegramBot)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
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

        var keyboard = TelegramKeyboards.GetExchangeSelectingKeyboard(true);

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId,
            "Выберите биржу из списка!",
            replyMarkup: keyboard);

        return _stateFactory.CreateState(BotState.WaitingForSelectingExchange);
    }
}