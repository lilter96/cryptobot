namespace CryptoBot.TelegramBot.BotStates
{
    public enum BotState
    {
        WaitingForCommand,
        WaitingForSymbol,
        WaitingForSelectingAccount,
        WaitingForSelectingExchange,
        WaitingForExchangeApiKeyState,
        WaitingForExchangeApiSecretState,
    }
}
