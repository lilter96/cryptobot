using Microsoft.Extensions.Logging;
using OKX.Net.Clients;
using OKX.Net.Enums;
using OKX.Net.Objects;

namespace CryptoBot.Exchanges.Exchanges.Clients
{
    public class OKXApiClient : IExchangeApiClient<OKXApiCredentials>
    {
        private readonly ILogger<OKXApiClient> _logger;

        public OKXApiClient(ILogger<OKXApiClient> logger)
        {
            _logger = logger;
        }

        public async Task<decimal> GetLastTradedPriceAsync(OKXApiCredentials apiCredentials, string symbol = "BTCUSDT")
        {
            var restApiClient = new OKXRestClient(options =>
            {
                options.ApiCredentials = apiCredentials;
            });

            var date = DateTime.UtcNow;

            var result = await restApiClient.UnifiedApi.ExchangeData.GetKlinesAsync(symbol, OKXPeriod.FiveMinutes, date);

            if (result.Error != null || !result.Success)
            {
                var errorMessage =
                    $"Something went wrong while receiving Last Traded Price for symbol {symbol}. Error Message: {result.Error?.Message}";
                _logger.LogError(errorMessage);

                throw new InvalidOperationException(errorMessage);
            }

            return result.Data.Last().ClosePrice;
        }

        public async Task<decimal> GetFeeRateAsync(OKXApiCredentials apiCredentials)
        {
            var restApiClient = new OKXRestClient(options =>
            {
                options.ApiCredentials = apiCredentials;
            });

            var result = await restApiClient.UnifiedApi.Account.GetFeeRatesAsync(OKXInstrumentType.Any);

            if (result.Error != null || !result.Success)
            {
                var errorMessage =
                    $"Something went wrong while receiving trading fees. Error Message: {result.Error?.Message}";
                _logger.LogError(errorMessage);

                throw new InvalidOperationException(errorMessage);
            }

            return result.Data.Maker ?? 0;
        }
    }
}
