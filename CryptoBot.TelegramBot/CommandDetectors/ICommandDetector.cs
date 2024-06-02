using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public interface ICommandDetector
{
    public CommandDescription CommandDescription { get; }

    Task<IBotState> TryDetectCommand(Update receivedUpdate);
}
