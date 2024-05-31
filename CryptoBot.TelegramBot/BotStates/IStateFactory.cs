using CryptoBot.Data.Entities;

namespace CryptoBot.TelegramBot.BotStates;

public interface IStateFactory
{
    IBotState CreateState(BotState state);
}