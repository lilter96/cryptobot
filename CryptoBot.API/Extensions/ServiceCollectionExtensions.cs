using CryptoBot.TelegramBot.BotStates.Factory;

namespace CryptoBot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeyedBotStates(this IServiceCollection services)
        {
            var botStateTypes = typeof(IBotState).Assembly.GetTypes()
                .Where(t => typeof(IBotState).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToList();

            var descriptorsToRemove = new List<ServiceDescriptor>();

            foreach (var botStateType in botStateTypes)
            {
                var descriptor = new ServiceDescriptor(botStateType, botStateType, ServiceLifetime.Transient);
                services.Add(descriptor);
                descriptorsToRemove.Add(descriptor);

                var serviceProvider = services.BuildServiceProvider();
                var botStateInstance = ActivatorUtilities.CreateInstance(serviceProvider, botStateType) as IBotState;

                if (botStateInstance == null)
                {
                    throw new InvalidOperationException($"Unable to create an instance of {botStateType.FullName}.");
                }

                var botStateKey = botStateInstance.BotState;

                services.AddKeyedTransient(typeof(IBotState), botStateKey, botStateType);
            }

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            return services;
        }
    }
}
