using CryptoBot.Exchanges.Exchanges.Clients;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class ReadSymbolCommandDetector : ICommandDetector
    {
        private readonly BybitApiClient _bybitApiClient;

        public ReadSymbolCommandDetector(BybitApiClient bybitApiClient)
        {
            _bybitApiClient = bybitApiClient;
        }

        public async Task<CommandData> TryDetectCommand(
            Update receivedUpdate,
            Classes.TelegramBot telegramBot)
        {
            var lastCommand = telegramBot.LastBotsCommands.Last();

            if (lastCommand != BotCommand.Price)
            {
                return null;
            }

            var receivedTelegramMessage = receivedUpdate.Message;
            var message = receivedTelegramMessage?.Text;

            try
            {
                var result = await _bybitApiClient.GetLastTradedPrice(message);

                await telegramBot.SendDefaultMessageAsync(
                    $"Последняя цена - {result}",
                    receivedTelegramMessage.Chat.Id);

                return new CommandData { Command = BotCommand.ReadSymbol };
            }
            catch (Exception e)
            {
                await telegramBot.SendDefaultMessageAsync(
                    "Выберите криптовалютную пару! А не залупу мамаши своей",
                    receivedTelegramMessage.Chat.Id);

                return null;
            }
        }
    }
}
