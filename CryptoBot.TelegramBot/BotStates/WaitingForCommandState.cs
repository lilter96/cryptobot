﻿using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates;

public class WaitingForCommandState : IBotState
{
    private readonly CommandDetectorService _commandDetectorService;
    private readonly TelegramBot _telegramBot;

    public WaitingForCommandState(CommandDetectorService commandDetectorService, TelegramBot telegramBot)
    {
        _commandDetectorService = commandDetectorService;
        _telegramBot = telegramBot;
    }


    public BotState BotState { get; set; } = BotState.WaitingForCommand;

    public async Task<IBotState> HandleUpdateAsync(Update update)
    {
        if (update.Message?.From == null || update.Message.From.Id == _telegramBot.BotClient.BotId)
        {
            return this;
        }

        var possibleNewBotState = await _commandDetectorService.DetectCommand(update);

        var chatId = update.GetChatId();

        if (possibleNewBotState == null)
        {
            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text:  "Неизвестная команда, попробуйте другую или введите команду /help",
                replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

            return this;
        }

        return possibleNewBotState;
    }
}
