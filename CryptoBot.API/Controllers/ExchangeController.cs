using Microsoft.AspNetCore.Mvc;

namespace CryptoBot.API.Controllers;

[ApiController]
[Route("exchange")]
public class ExchangeController : ControllerBase
{
    private readonly TelegramBot.TelegramBot _telegramBot;

    public ExchangeController(TelegramBot.TelegramBot telegramBot)
    {
        _telegramBot = telegramBot;
    }


    [HttpGet]
    [Route("asd")]
    public async Task<IActionResult> StartBot()
    {
        await _telegramBot.StartReceivingMessagesAsync();

        return Ok();
    }
}
