using CryptoBot.Exchanges.Exchanges.Clients;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBot.API.Controllers
{
    [ApiController]
    [Route("exchange")]
    public class ExchangeController : ControllerBase
    {
        private readonly BybitApiClient _bybitApiClient;
        private readonly TelegramBot.TelegramBot _telegramBot;

        public ExchangeController(BybitApiClient bybitApiClient, TelegramBot.TelegramBot telegramBot)
        {
            _bybitApiClient = bybitApiClient;
            _telegramBot = telegramBot;
        }

        [HttpGet]
        [Route("hui")]
        public async Task<IActionResult> GetLastTradedPrice(string symbol = "BTCUSDT")
        {
            var result = await _bybitApiClient.GetLastTradedPrice(symbol);

            return Ok(result);
        }

        [HttpGet]
        [Route("asd")]
        public async Task<IActionResult> StartBot()
        {
            await _telegramBot.StartReceivingMessagesAsync();

            return Ok();
        }
    }
}
