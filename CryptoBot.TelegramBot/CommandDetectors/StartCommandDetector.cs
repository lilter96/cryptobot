using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class StartCommandDetector : ICommandDetector
{
    public CommandDescription CommandDescription { get; } =
        new() { Command = "/start", Description = "Запустить бота" };

    public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
    {
        var chatId = receivedUpdate.GetChatId();

        if (receivedUpdate?.Message == null)
        {
            return null;
        }

        var message = receivedUpdate.Message.Text;

        if (message == CommandDescription.Command)
        {

        }

        return await Task.FromResult((IBotState) null);
    }
}
