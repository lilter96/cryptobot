using CryptoBot.Service.Services.Account;
using CryptoBot.Service.Services.Cryptography;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForExchangeApiKeyState : IBotState
{
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly ICryptographyService _cryptographyService;
    private readonly IStateFactory _stateFactory;
    private readonly IAccountService _accountService;

    public WaitingForExchangeApiKeyState(
        ILogger<WaitingForSymbolState> logger,
        TelegramBot telegramBot,
        ICryptographyService cryptographyService,
        IStateFactory stateFactory,
        IAccountService accountService)
    {
        _logger = logger;
        _telegramBot = telegramBot;
        _cryptographyService = cryptographyService;
        _stateFactory = stateFactory;
        _accountService = accountService;
    }

    public BotState BotState { get; set; } = BotState.WaitingForExchangeApiKeyState;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        var apiKey = update?.Message?.Text;

        var chatId = update.GetChatId();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        var encryptedApiKey = await _cryptographyService.EncryptAsync(apiKey);

        var patchDocument = new JsonPatchDocument();
        patchDocument.Replace("/EncryptedApiKey", encryptedApiKey);

        await _accountService.PatchSelectedAccountAsync(chatId, patchDocument);

        await _telegramBot.BotClient.DeleteMessageAsync(chatId, update.Message.MessageId);

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: "API Key принят.",
            replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Введите API Secret! ВНИМАНИЕ: используйте ключ только для чтения",
            replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

        return _stateFactory.CreateState(BotState.WaitingForExchangeApiSecretState);
    }
}
