using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class PriceCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;

    public PriceCommand(IStateFactory stateFactory, TelegramBot telegramBot)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
    }

    public CommandDescription CommandDescription { get; } =
        new() { Command = "/getprice", Description = "Получить текущую цену криптовалютной пары" };

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

        if (text == CommandDescription.Command)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите криптовалютную пару!",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            return _stateFactory.CreateState(BotState.WaitingForSymbol);
        }

        return null;
    }
}
