using CryptoBot.Exchanges.Exchanges.Clients;
using Telegram.Bot.Types;

namespace CryptoBot.TelegramBot.BotStates
{
    public class WaitingForSymbolState : IBotState
    {
        private readonly BybitApiClient _bybitApiClient;
        private readonly IStateFactory _stateFactory;

        public WaitingForSymbolState(BybitApiClient bybitApiClient, IStateFactory stateFactory)
        {
            _bybitApiClient = bybitApiClient;
            _stateFactory = stateFactory;
        }

        public BotCommand? Command { get; set; } = BotCommand.GetLastTradedPriceOfSymbol;

        public async Task<IBotState> HandleUpdateAsync(Update update, Classes.TelegramBot telegramBot)
        {
            var message = update.Message?.Text ?? string.Empty;

            var chatId = update.GetChatId();

            if (string.IsNullOrWhiteSpace(message))
            {
                await telegramBot.SendDefaultMessageAsync("Некорректный ввод, попробуйте снова.", chatId);
                return this;
            }

            try
            {
                var result = await _bybitApiClient.GetLastTradedPrice(message);
                await telegramBot.SendDefaultMessageAsync($"Последняя цена - {result}", chatId);
                return _stateFactory.CreateState<WaitingForCommandState>();
            }
            catch (InvalidOperationException ex)
            {
                await telegramBot.SendDefaultMessageAsync($"Выбранная вами криптовалютная пара {message} не поддерживается", chatId);
                return this;
            }
            catch (Exception)
            {
                await telegramBot.SendDefaultMessageAsync($"Внутренняя ошибка, попробуйте позже еще раз!", chatId);
                return this;
            }
        }
    }
}
