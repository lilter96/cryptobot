using CryptoBot.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoBot.TelegramBot.BotStates;

public class StateFactory : IStateFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StateFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IBotState CreateState(BotState state)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredKeyedService<IBotState>(state);
    }
}