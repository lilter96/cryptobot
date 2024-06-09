using CryptoBot.Data.Entities;

namespace CryptoBot.TelegramBot.BotStates.Factory;

public interface IStateFactory
{
    IBotState CreateState(BotState state);
}