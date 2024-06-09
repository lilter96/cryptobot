namespace CryptoBot.Data.Entities;

public enum BotState
{
    WaitingForCommand,
    WaitingForSymbol,
    WaitingForSelectingAccount,
    WaitingForSelectingExchange,
    WaitingForExchangeApiKeyState,
    WaitingForExchangeApiSecretState,
}
