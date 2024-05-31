using CryptoBot.Data.Entities;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public interface IBotState
{
    public BotState BotState { get; set; }

    Task<IBotState> HandleUpdateAsync(Update update);
}