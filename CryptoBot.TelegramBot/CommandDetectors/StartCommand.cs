using System.ComponentModel.DataAnnotations;
using CryptoBot.Service.Services.Chat;
using CryptoBot.TelegramBot.BotStates;
using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using CryptoBot.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class StartCommand : ICommandDetector
{
    private readonly IStateFactory _stateFactory;
    private readonly TelegramBot _telegramBot;
    private readonly IChatService _chatService;

    public StartCommand(
        IStateFactory stateFactory,
        TelegramBot telegramBot,
        IChatService chatService)
    {
        _stateFactory = stateFactory;
        _telegramBot = telegramBot;
        _chatService = chatService;
    }

    public CommandDescription CommandDescription { get; } =
        new() { Command = "/start", Description = "Запустить бота" };

    public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
    {
        var receivedTelegramMessage = receivedUpdate.Message;
        var chatId = receivedUpdate.GetChatId();

        var text = receivedTelegramMessage.Text;

        if (text != CommandDescription.Command)
        {
            throw new ValidationException();
        }

        var isStartMessage = receivedTelegramMessage is { Text: "/start" };

        if (isStartMessage)
        {
            await _chatService.CreateChatAsync(chatId);
        }

        await _telegramBot.BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Доброго времени суток, с вами БОТ КРИПТОПАМПИКС!",
            replyMarkup: TelegramKeyboards.GetDefaultKeyboard());

        return _stateFactory.CreateState(BotState.WaitingForCommand);
    }
}
