namespace CryptoBot.TelegramBot.BotStates
{
    public interface IStateFactory
    {
        IBotState CreateState<T>() where T : IBotState;
    }
}
