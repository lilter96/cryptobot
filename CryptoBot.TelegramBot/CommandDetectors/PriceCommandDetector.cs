using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class CommandData
    {
        public BotCommand Command { get; set; }

        public object Data { get; set; }
    }

    public interface ICommandDetector
    {
        Task<CommandData> TryDetectCommand(Update receivedUpdate, Classes.TelegramBot telegramBot);
    }

    public class PriceCommandDetector : ICommandDetector
    {
        public async Task<CommandData> TryDetectCommand(Update receivedUpdate, Classes.TelegramBot telegramBot)
        {
            var receivedTelegramMessage = receivedUpdate.Message;

            var text = receivedTelegramMessage.Text;

            if (text == "/price")
            {
                await telegramBot.SendDefaultMessageAsync("Выберите криптовалютную пару!", receivedTelegramMessage.Chat.Id);

                return new CommandData { Command = BotCommand.Price, Data = text };
            }

            return null;
        }
    }
}
