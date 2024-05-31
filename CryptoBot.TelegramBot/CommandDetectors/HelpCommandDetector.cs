using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class HelpCommandDetector : ICommandDetector
    {
        private readonly IStateFactory _stateFactory;

        public HelpCommandDetector(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public async Task<IBotState> TryDetectCommand(Update receivedUpdate, Classes.TelegramBot telegramBot)
        {
            var receivedTelegramMessage = receivedUpdate.Message;

            var chatId = receivedUpdate.GetChatId();

            if (receivedTelegramMessage == null)
            {
                await telegramBot.SendDefaultMessageAsync("Вы сделали все что угодно, но не отправили мне комманду!", chatId);
                return null;
            }

            var text = receivedTelegramMessage.Text;

            if (text == "/price")
            {
                await telegramBot.SendDefaultMessageAsync("Выберите криптовалютную пару!", chatId);

                return _stateFactory.CreateState<WaitingForSymbolState>();
            }

            return null;
        }
    }
}
