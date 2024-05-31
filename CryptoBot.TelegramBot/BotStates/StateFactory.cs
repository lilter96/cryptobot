using Microsoft.Extensions.DependencyInjection;

namespace CryptoBot.TelegramBot.BotStates
{
    public class StateFactory : IStateFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public StateFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBotState CreateState<T>() where T : IBotState
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
