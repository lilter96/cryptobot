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

        public BotCommand? Command { get; set; } = null;

        public async Task<IBotState> HandleUpdateAsync(Update update, Classes.TelegramBot telegramBot)
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
