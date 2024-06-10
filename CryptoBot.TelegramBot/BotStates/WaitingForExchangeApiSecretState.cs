using CryptoBot.Service.Services.Account;
using CryptoBot.Service.Services.Cryptography;
using CryptoBot.Service.Services.ExchangeApi;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForExchangeApiSecretState : IBotState
{
    private readonly ILogger<WaitingForExchangeApiSecretState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly ICryptographyService _cryptographyService;
    private readonly IStateFactory _stateFactory;
    private readonly IExchangeApiService _exchangeApiService;
    private readonly IAccountService _accountService;

    public WaitingForExchangeApiSecretState(
        ILogger<WaitingForExchangeApiSecretState> logger,
        TelegramBot telegramBot,
        ICryptographyService cryptographyService,
        IStateFactory stateFactory,
        IExchangeApiService exchangeApiService,
        IAccountService accountService)
    {
        _logger = logger;
        _telegramBot = telegramBot;
        _cryptographyService = cryptographyService;
        _stateFactory = stateFactory;
        _exchangeApiService = exchangeApiService;
        _accountService = accountService;
    }

    public BotState BotState { get; set; } = BotState.WaitingForExchangeApiSecretState;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        var apiSecret = update?.Message?.Text;

        var chatId = update.GetChatId();

        if (string.IsNullOrWhiteSpace(apiSecret))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        var encryptedSecretKey = await _cryptographyService.EncryptAsync(apiSecret);

        var patchDocument = new JsonPatchDocument();
        patchDocument.Replace("/EncryptedApiSecret", encryptedSecretKey);

        await _accountService.PatchSelectedAccountAsync(chatId, patchDocument);

        await _telegramBot.BotClient.DeleteMessageAsync(chatId, update.Message.MessageId);
        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: "API Secret принят.",
            replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

        var selectedAccount = await _accountService.GetSelectedAccountAsync(chatId);

        try
        {
            await _exchangeApiService.PingAsync(chatId);

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вы успешно добавили аккаунт биржи {selectedAccount.Exchange}",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            var message = await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Текущий аккаунт: {selectedAccount.Exchange}, id: {selectedAccount.Id}",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            await _telegramBot.BotClient.UnpinAllChatMessages(chatId);
            await _telegramBot.BotClient.PinChatMessageAsync(chatId, message.MessageId, true);
        }
        catch (Exception)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы ввели некорректные данные от аккаунта, попробуйте еще раз!",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            await _accountService.DeleteSelectedAccountAsync(chatId);
        }

        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}
