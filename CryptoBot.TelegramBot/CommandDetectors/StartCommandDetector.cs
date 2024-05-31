using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class StartCommandDetector : ICommandDetector
    {
        public async Task<IBotState> TryDetectCommand(Update receivedUpdate, TelegramBot telegramBot)
        {
            var chatId = receivedUpdate.GetChatId();

            if (receivedUpdate?.Message == null)
            {
                return null;
            }

            var message = receivedUpdate.Message.Text;

            if (message == "/start")
            {

            }

            return await Task.FromResult((IBotState) null);
        }
    }
}
