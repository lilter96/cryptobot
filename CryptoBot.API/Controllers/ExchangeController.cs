using CryptoBot.Exchanges.Exchanges.Clients;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBot.API.Controllers
{
    [ApiController]
    [Route("exchange")]
    public class ExchangeController : ControllerBase
    {
        private readonly BybitApiClient _bybitApiClient;

        public ExchangeController(BybitApiClient bybitApiClient)
        {
            _bybitApiClient = bybitApiClient;
        }

        [HttpGet]
        [Route("hui")]
        public async Task<IActionResult> GetLastTradedPrice(string symbol = "BTCUSDT")
        {
            var result = await _bybitApiClient.GetLastTradedPrice(symbol);

            return Ok(result);
        }
    }
}
