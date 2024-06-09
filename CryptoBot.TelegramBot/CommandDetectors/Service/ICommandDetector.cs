using CryptoBot.TelegramBot.BotStates.Factory;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors.Service;

public interface ICommandDetector
{
    public CommandDescription CommandDescription { get; }

    Task<IBotState> TryDetectCommand(Update receivedUpdate);
}
