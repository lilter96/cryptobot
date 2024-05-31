using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.BotStates;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class RegisterExchangeCommandDetector : ICommandDetector
    {
        private readonly IStateFactory _stateFactory;
        private readonly TelegramBot _telegramBot;

        public RegisterExchangeCommandDetector(IStateFactory stateFactory, TelegramBot telegramBot)
        {
            _stateFactory = stateFactory;
            _telegramBot = telegramBot;
        }

        public async Task<IBotState> TryDetectCommand(Update receivedUpdate)
        {
            var receivedTelegramMessage = receivedUpdate.Message;

            var chatId = receivedUpdate.GetChatId();

            if (receivedTelegramMessage == null)
            {
                await _telegramBot.SendDefaultMessageAsync("Вы сделали все что угодно, но не отправили мне комманду!", chatId);
                return null;
            }

            var text = receivedTelegramMessage.Text;

            if (text != "/registerExchange")
            {
                return null;
            }

            // Buttons
            var urlButton = new KeyboardButton("Bybit");
            var urlButton2 = new KeyboardButton("Binance");

            var buttons = new[] { urlButton, urlButton2 };

            // Keyboard markup
            var inline = new ReplyKeyboardMarkup(buttons);

            await _telegramBot.BotClient.SendTextMessageAsync(
                chatId,
                "Выберите биржу из списка!",
                replyMarkup: inline);

            return _stateFactory.CreateState(BotState.WaitingForSelectingExchange);
        }
    }
}
