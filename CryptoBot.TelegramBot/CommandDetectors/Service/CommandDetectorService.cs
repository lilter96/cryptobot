using CryptoBot.TelegramBot.BotStates.Factory;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.CommandDetectors.Service;

public class CommandDetectorService
{
    private readonly List<Func<Update, Task<IBotState>>> _allDetectors = [];

    public readonly List<CommandDescription> AllCommands = [];

    public CommandDetectorService(IServiceProvider serviceProvider)
    {
        var detectorsTypes = typeof(PriceCommand).Assembly
            .GetTypes()
            .Where(x => !x.IsInterface && x.IsAssignableTo(typeof(ICommandDetector)))
            .ToList();

        foreach (var detectorType in detectorsTypes)
        {
            if (serviceProvider.GetRequiredService(detectorType) is not ICommandDetector detector)
            {
                continue;
            }

            _allDetectors.Add(detector.TryDetectCommand);
            AllCommands.Add(detector.CommandDescription);
        }
    }


    public async Task<IBotState> DetectCommand(Update update)
    {
        foreach (var detectionFunc in _allDetectors)
        {
            var possibleNewBotState = await detectionFunc(update);
            if (possibleNewBotState != null)
            {
                return possibleNewBotState;
            }
        }

        return null;
    }
}
