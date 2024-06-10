using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Exchanges;
using CryptoBot.Service.Services.Cryptography;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OKX.Net.Objects;

namespace CryptoBot.Service.Services.ExchangeApi
{
    public class ExchangeApiService : IExchangeApiService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CryptoBotDbContext _dbContext;
        private readonly ICryptographyService _cryptographyService;

        public ExchangeApiService(
            IServiceProvider serviceProvider,
            CryptoBotDbContext dbContext, ICryptographyService cryptographyService)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _cryptographyService = cryptographyService;
        }

        public async Task PingAsync(long chatId)
        {
            var chat = await _dbContext.Chats
                .Include(x => x.SelectedAccount)
                .FirstOrDefaultAsync(x => x.Id == chatId);

            var exchangeApiClient = GetApiClientByChatId(chat.SelectedAccount.Exchange);

            var apiCredentials = await GetApiCredentialsByAccount(chat.SelectedAccount);

            await exchangeApiClient.GetFeeRateAsync(apiCredentials);
        }

        public async Task<decimal> GetSymbolPrice(long chatId,  string symbol)
        {
            var chat = await _dbContext.Chats
                .Include(x => x.SelectedAccount)
                .FirstOrDefaultAsync(x => x.Id == chatId);

            var exchangeApiClient = GetApiClientByChatId(chat.SelectedAccount.Exchange);

            var apiCredentials = await GetApiCredentialsByAccount(chat.SelectedAccount);

            return await exchangeApiClient.GetLastTradedPriceAsync(apiCredentials, symbol);
        }

        public List<string> GetExchangeNames() =>
            Enum.GetValues(typeof(Exchange))
                .Cast<Exchange>()
                .Select(exchange => exchange.ToString())
                .ToList();

        private IExchangeApiClient<ApiCredentials> GetApiClientByChatId(Exchange exchange)
        {
            var exchangeApiClient = _serviceProvider.GetRequiredKeyedService<IExchangeApiClient<ApiCredentials>>(exchange);

            return exchangeApiClient;
        }

        private async Task<ApiCredentials> GetApiCredentialsByAccount(AccountEntity entity)
        {
            var apiKey = await _cryptographyService.DecryptAsync(entity.EncryptedApiKey);
            var apiSecret = await _cryptographyService.DecryptAsync(entity.EncryptedApiSecret);

            switch (entity.Exchange)
            {
                case Exchange.Bybit:
                case Exchange.Binance:
                    return new ApiCredentials(apiKey, apiSecret);
                case Exchange.OKX:
                    var apiPassPhrase = await _cryptographyService.DecryptAsync(entity.EncryptedApiPassPhrase);
                    return new OKXApiCredentials(apiKey, apiSecret, apiPassPhrase);
                default:
                    throw new InvalidOperationException("Exchange is not supported");
            }
        }
    }
}
