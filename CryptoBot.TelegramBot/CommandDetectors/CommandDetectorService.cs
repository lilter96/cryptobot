using CryptoBot.TelegramBot.BotStates;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors;

public class CommandDetectorService
{
    private readonly List<Func<Update, Task<IBotState>>> _allDetectors = [];

    public CommandDetectorService(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var detectorsTypes = typeof(PriceCommandDetector).Assembly
            .GetTypes()
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

    public async Task<IBotState> DetectCommand(Update update)
    {
        foreach (var x in _allDetectors)
        {
            var possibleNewBotState = await x(update);
            if (possibleNewBotState != null)
            {
                return possibleNewBotState;
            }
        }

        return null;
    }
}