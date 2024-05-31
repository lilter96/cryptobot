using CryptoBot.Data.Entities;
using CryptoBot.TelegramBot.CommandDetectors;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates
{
    public class WaitingForCommandState : IBotState
    {
        private readonly CommandDetectorService _commandDetectorService;

        public WaitingForCommandState(CommandDetectorService commandDetectorService)
        {
            _commandDetectorService = commandDetectorService;
        }


        public BotState BotState { get; set; } = BotState.WaitingForCommand;

        public async Task<IBotState> HandleUpdateAsync(Update update, TelegramBot telegramBot)
        {
            var possibleNewBotState = await _commandDetectorService.DetectCommand(update, telegramBot);

            var chatId = update.GetChatId();

            if (possibleNewBotState == null)
            {
                await telegramBot.SendDefaultMessageAsync(
                    "Неизвестная команда, попробуйте другую или введите команду /help", chatId);

                return this;
            }

            return possibleNewBotState;
        }
    }
}
