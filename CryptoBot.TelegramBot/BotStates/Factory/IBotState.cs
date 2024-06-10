using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates.Factory;

public interface IBotState
{
    public BotState BotState { get; set; }

    Task<IBotState> HandleUpdateAsync(Update update);
}
