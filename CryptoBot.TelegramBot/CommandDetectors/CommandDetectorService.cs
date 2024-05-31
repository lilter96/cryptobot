using CryptoBot.TelegramBot.BotStates;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors
{
    public class CommandDetectorService
    {
        private readonly List<Func<Update, TelegramBot, Task<IBotState>>> _allDetectors = [];

        public CommandDetectorService(IServiceScopeFactory serviceScopeFactory)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var detectorsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => !x.IsInterface && x.IsAssignableTo(typeof(ICommandDetector)))
                .ToList();

            foreach (var detectorType in detectorsTypes)
            {
                 if (scope.ServiceProvider.GetRequiredService(detectorType) is not ICommandDetector detector)
                 {
                     continue;
                 }

                 _allDetectors.Add(detector.TryDetectCommand);
            }
        }

        public async Task<IBotState> DetectCommand(Update update, TelegramBot telegramBot)
        {
            foreach (var x in _allDetectors)
            {
                var possibleNewBotState = await x(update, telegramBot);
                if (possibleNewBotState != null)
                {
                    return possibleNewBotState;
                }
            }

            return null;
        }
    }
}
