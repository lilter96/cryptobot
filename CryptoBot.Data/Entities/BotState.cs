namespace CryptoBot.Data.Entities;

public enum BotState
{
    WaitingForCommand,
    WaitingForSymbol,
    WaitingForSelectingExchange,
    WaitingForExchangeApiKeyState,
    WaitingForExchangeApiSecretState,
}
