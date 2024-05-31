using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace CryptoBot.Exchanges.Exchanges.Clients;

public class BybitApiClient
{
    private readonly IBybitRestClient _restApiClient;
    private readonly ILogger<BybitApiClient> _logger;

    public BybitApiClient(IBybitRestClient restApiClient, ILogger<BybitApiClient> logger)
    {
        _restApiClient = restApiClient;
        _logger = logger;
    }

    public async Task<decimal> GetLastTradedPrice(string symbol)
    {
        var date = DateTime.UtcNow;

        var result = await _restApiClient.V5Api.ExchangeData.GetKlinesAsync(Category.Spot, symbol.ToUpper(), KlineInterval.FiveMinutes, date);

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