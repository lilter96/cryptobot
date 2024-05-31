using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class PriceCommandDetector : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
        
    public PriceCommandDetector(IStateFactory stateFactory, TelegramBot telegramBot)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
    }

    public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
    {
        var receivedTelegramMessage = receivedUpdate.Message;

        var chatId = receivedUpdate.GetChatId();

        if (receivedTelegramMessage == null)
        {
            await _telegramBot.SendDefaultMessageAsync("Вы сделали все что угодно, но не отправили мне комманду!", chatId);
            return null;
        }

        var text = receivedTelegramMessage.Text;

        if (text == "/price")
        {
            await _telegramBot.SendDefaultMessageAsync("Выберите криптовалютную пару!", chatId);

            return _stateFactory.CreateState(BotState.WaitingForSymbol);
        }

        return null;
    }
}