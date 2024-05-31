using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class PriceCommandDetector : ICommandDetector
    {
        private readonly IStateFactory _stateFactory;

        public PriceCommandDetector(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public async Task<IBotState> TryDetectCommand(Update receivedUpdate, TelegramBot telegramBot)
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

                return _stateFactory.CreateState(BotState.WaitingForSymbol);
            }

            return null;
        }
    }
}
