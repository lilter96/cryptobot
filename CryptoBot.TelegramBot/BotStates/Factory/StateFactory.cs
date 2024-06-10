using Microsoft.Extensions.DependencyInjection;

namespace CryptoBot.TelegramBot.BotStates.Factory;

public class StateFactory : IStateFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StateFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBotState CreateState(BotState state)
    {
        return _serviceProvider.GetRequiredKeyedService<IBotState>(state);
    }
}
