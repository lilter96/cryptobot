namespace CryptoBot.Service.Services.ExchangeApi
{
    public interface IExchangeApiService
    {
        Task PingAsync(long chatId);

        Task<decimal> GetSymbolPrice(long chatId, string symbol);

        List<string> GetExchangeNames();
    }
}
