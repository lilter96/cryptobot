using CryptoBot.Service.Services.Account;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.String;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSelectingExchangeState : IBotState
{
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly IAccountService _accountService;

    public WaitingForSelectingExchangeState(
        IStateFactory stateFactory,
        ILogger<WaitingForSymbolState> logger,
        TelegramBot telegramBot,
        IAccountService accountService)
    {
        _stateFactory = stateFactory;
        _logger = logger;
        _telegramBot = telegramBot;
        _accountService = accountService;
    }

    public BotState BotState { get; set; } = BotState.WaitingForSelectingExchange;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        var message = update?.Message?.Text;

        var chatId = update.GetChatId();

        if (IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        try
        {
            var patchDocument = new JsonPatchDocument();
            patchDocument.Replace("/Exchange", message);

            await _accountService.PatchSelectedAccountAsync(chatId, patchDocument);

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Введите API Key! ВНИМАНИЕ: используйте ключ только для чтения",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            return _stateFactory.CreateState(BotState.WaitingForExchangeApiKeyState);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
