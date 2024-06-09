using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForSelectingAccountState : IBotState
{
    private readonly IStateFactory _stateFactory;
    private readonly ILogger<WaitingForSymbolState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WaitingForSelectingAccountState(
        IStateFactory stateFactory,
        ILogger<WaitingForSymbolState> logger,
        TelegramBot telegramBot,
        IServiceScopeFactory serviceScopeFactory)
    {
        _stateFactory = stateFactory;
        _logger = logger;
        _telegramBot = telegramBot;
        _serviceScopeFactory = serviceScopeFactory;
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

        var isGuid = Guid.TryParse(callbackData, out var guid);

        if (!isGuid)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Что-то пошло не так, попробуйте снова.",
                replyMarkup: TelegramKeyboards.GetExchangeSelectingKeyboard(true));

            return await Task.FromResult((IBotState) null);
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

            var account = await dbContext.Accounts
                .Include(accountEntity => accountEntity.Chat)
                .Include(accountEntity => accountEntity.Exchange)
                .FirstOrDefaultAsync(x =>
                    x.Id == guid &&
                    x.ChatId == chatId);

            account.Chat.SelectedAccountId = account.Id;
            dbContext.Update(account.Chat);

            await dbContext.SaveChangesAsync();

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Аккаунт успешно переключен.",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            var message = await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Текущий аккаунт: {account.Chat.SelectedAccount.Exchange.Exchange.ToString()}, id: {account.Chat.SelectedAccountId}",
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
