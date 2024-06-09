using CryptoBot.TelegramBot.BotStates.Factory;
using CryptoBot.TelegramBot.CommandDetectors.Service;
using Telegram.Bot;

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

        public static IServiceCollection AddCommandDetectors(this IServiceCollection services)
        {
            var detectorsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => !x.IsInterface && x.IsAssignableTo(typeof(ICommandDetector)))
                .ToList();

            foreach (var detector in detectorsTypes)
            {
                services.AddTransient(detector);
            }

            return services;
        }

        public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(configuration["TelegramBotConfiguration:TelegramBotToken"]!));
            services.AddTransient<TelegramBot.TelegramBot>();

            services.AddTransient<IStateFactory, StateFactory>();

            services.AddSingleton<CommandDetectorService>();
            services.AddCommandDetectors();

            services.AddKeyedBotStates();

            return services;
        }
    }
}
