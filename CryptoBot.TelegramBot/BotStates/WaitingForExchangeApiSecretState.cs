﻿using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Exchanges.Exchanges.Clients;
using CryptoBot.Service.Services.Interfaces;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.Keyboards;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForExchangeApiSecretState : IBotState
{
    private readonly ILogger<WaitingForExchangeApiSecretState> _logger;
    private readonly TelegramBot _telegramBot;
    private readonly ICryptoService _cryptoService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IStateFactory _stateFactory;
    private readonly BybitApiClient _bybitApiClient;

    public WaitingForExchangeApiSecretState(ILogger<WaitingForExchangeApiSecretState> logger, TelegramBot telegramBot, ICryptoService cryptoService, IServiceScopeFactory serviceScopeFactory, IStateFactory stateFactory, BybitApiClient bybitApiClient)
    {
        _logger = logger;
        _telegramBot = telegramBot;
        _cryptoService = cryptoService;
        _serviceScopeFactory = serviceScopeFactory;
        _stateFactory = stateFactory;
        _bybitApiClient = bybitApiClient;
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

        var encryptedSecretKey = await _cryptoService.EncryptAsync(apiSecret);

        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<CryptoBotDbContext>();

        var chat = await dbContext.Chats
            .Include(chatEntity => chatEntity.SelectedAccount)
            .ThenInclude(x => x.Exchange)
            .FirstOrDefaultAsync(x => x.Id == chatId);

        chat.SelectedAccount.Exchange.EncryptedSecret = encryptedSecretKey;

        await dbContext.SaveChangesAsync();

        await _telegramBot.BotClient.DeleteMessageAsync(chatId, update.Message.MessageId);
        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: "API Secret принят.",
            replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

        var decryptedApiKey = await _cryptoService.DecryptAsync(chat.SelectedAccount.Exchange.EncryptedKey);

        var apiCredentials = new ApiCredentials(decryptedApiKey, apiSecret);

        try
        {
            _ = await _bybitApiClient.GetFeeRateAsync(apiCredentials);

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вы успешно добавили аккаунт биржи {chat.SelectedAccount.Exchange.Exchange}",
                replyMarkup: TelegramKeyboards.GetEmptyKeyboard());

            var message = await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Текущий аккаунт: {chat.SelectedAccount.Exchange.Exchange.ToString()}, id: {chat.SelectedAccountId}",
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

            dbContext.Accounts.Remove(chat.SelectedAccount);

            chat.SelectedAccountId = null;

            await dbContext.SaveChangesAsync();
        }

        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}
