using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;

namespace CryptoBot.Exchanges.Exchanges.Clients;

public class BybitApiClient
{
    private readonly ILogger<BybitApiClient> _logger;

    public BybitApiClient(
        ILogger<BybitApiClient> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> GetLastTradedPrice(ApiCredentials apiCredentials, string symbol)
    {
        var restApiClient = new BybitRestClient(options =>
        {
            options.ApiCredentials = apiCredentials;
        });

        var date = DateTime.UtcNow;

        var result = await restApiClient.V5Api.ExchangeData.GetKlinesAsync(Category.Spot, symbol.ToUpper(), KlineInterval.FiveMinutes, date);

        if (result.Error != null)
        {
            var errorMessage =
                $"Something went wrong while receiving Last Traded Price for symbol {symbol}. Error Message: {result.Error.Message}";
            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        return result.Data.List.Last().ClosePrice;
    }
}
