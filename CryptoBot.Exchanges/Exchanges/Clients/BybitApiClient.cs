using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;

namespace CryptoBot.Exchanges.Exchanges.Clients;

public class BybitApiClient : IExchangeApiClient<ApiCredentials>
{
    private readonly ILogger<BybitApiClient> _logger;

    public BybitApiClient(ILogger<BybitApiClient> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> GetLastTradedPriceAsync(ApiCredentials apiCredentials, string symbol = "BTCUSDT")
    {
        var restApiClient = new BybitRestClient(options =>
        {
            options.ApiCredentials = apiCredentials;
        });

        var date = DateTime.UtcNow;

        var result = await restApiClient.V5Api.ExchangeData.GetKlinesAsync(Category.Spot, symbol.ToUpper(), KlineInterval.FiveMinutes, date);

        if (result.Error != null || !result.Success)
        {
            var errorMessage =
                $"Something went wrong while receiving Last Traded Price for symbol {symbol}. Error Message: {result.Error?.Message}";
            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        return result.Data.List.Last().ClosePrice;
    }

    public async Task<decimal> GetFeeRateAsync(ApiCredentials apiCredentials)
    {
        var restApiClient = new BybitRestClient(options =>
        {
            options.ApiCredentials = apiCredentials;
        });

        var result = await restApiClient.V5Api.Account.GetFeeRateAsync(Category.Spot);

        if (result.Error != null || !result.Success)
        {
            var errorMessage =
                $"Something went wrong while receiving trading fees. Error Message: {result.Error?.Message}";
            _logger.LogError(errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        return result.Data.List.Last().MakerFeeRate;
    }
}
