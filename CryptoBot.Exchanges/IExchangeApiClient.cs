namespace CryptoBot.Exchanges
{
    public interface IExchangeApiClient<in TApiCredentials>
    {
        Task<decimal> GetLastTradedPriceAsync(TApiCredentials apiCredentials, string symbol = "BTCUSDT");

        Task<decimal> GetFeeRateAsync(TApiCredentials apiCredentials);
    }
}
