using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public interface ICommandDetector
    {
        Task<IBotState> TryDetectCommand(Update receivedUpdate, TelegramBot telegramBot);
    }
}
