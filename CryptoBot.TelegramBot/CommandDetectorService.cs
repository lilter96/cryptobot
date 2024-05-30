using CryptoBot.TelegramBot.CommandDetectors;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot
{
    public class CommandDetectorService
    {
        private readonly List<Func<Update, Classes.TelegramBot, Task<CommandData>>> _allDetectors = [];

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

        public async Task<BotCommand?> DetectCommand(Update update, Classes.TelegramBot telegramBot)
        {
            foreach (var x in _allDetectors)
            {
                var tryDetectCommand = await x(update, telegramBot);
                if (tryDetectCommand != null)
                {
                    return tryDetectCommand.Command;
                }
            }

            return null;
        }
    }
}
