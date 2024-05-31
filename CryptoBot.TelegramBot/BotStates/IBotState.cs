using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates
{
    public interface IBotState
    {
        public BotCommand? Command { get; set; }

        Task<IBotState> HandleUpdateAsync(Update update, Classes.TelegramBot telegramBot);
    }
}
