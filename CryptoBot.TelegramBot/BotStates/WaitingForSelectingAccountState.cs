using CryptoBot.Service.Services.Account;
using CryptoBot.Service.Services.ExchangeApi;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSelectingAccountState : IBotState
{
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly IAccountService _accountService;
    private readonly IExchangeApiService _exchangeApiService;

    public WaitingForSelectingAccountState(
        IStateFactory stateFactory,
        ILogger<WaitingForSymbolState> logger,
        TelegramBot telegramBot,
        IAccountService accountService,
        IExchangeApiService exchangeApiService)
    {
        _stateFactory = stateFactory;
        _logger = logger;
        _telegramBot = telegramBot;
        _accountService = accountService;
        _exchangeApiService = exchangeApiService;
    }

    public BotState BotState { get; set; } = BotState.WaitingForSelectingAccount;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        var callbackData = update?.CallbackQuery?.Data;

        var chatId = update.GetChatId();

        if (string.IsNullOrWhiteSpace(callbackData))
        {
            _logger.LogWarning("Empty message in update from Telegram");
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректный ввод, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        var isGuid = Guid.TryParse(callbackData, out var newSelectedAccountId);

        if (!isGuid)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Что-то пошло не так, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetExchangeSelectingKeyboard(_exchangeApiService.GetExchangeNames()));

            return await Task.FromResult((IBotState) null);
        }

        try
        {
            await _accountService.SelectAccountAsync(chatId, newSelectedAccountId);

            var selectedAccount = await _accountService.GetSelectedAccountAsync(chatId);

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Аккаунт успешно переключен.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            var message = await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Текущий аккаунт: {selectedAccount.Exchange}, id: {selectedAccount.Id}",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            await _telegramBot.BotClient.UnpinAllChatMessages(chatId);
            await _telegramBot.BotClient.PinChatMessageAsync(chatId, message.MessageId, true);

            return _stateFactory.CreateState(BotState.WaitingForCommand);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong while selecting new current account. Error details: {e.Message}");
            throw;
        }
    }
}
