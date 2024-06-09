using CryptoBot.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoBot.TelegramBot.Keyboards;

public static class TelegramKeyboards
{
    public static ReplyKeyboardMarkup GetDefaultKeyboard()
    {
        var buttons = new List<List<KeyboardButton>>
        {
            Capacity = 0
        };

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        return replyKeyboardMarkup;
    }

    public static ReplyKeyboardMarkup GetEmptyKeyboard()
    {
        var buttons = new List<List<KeyboardButton>>();
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons);

        return replyKeyboardMarkup;
    }

    public static ReplyKeyboardMarkup GetExchangeSelectingKeyboard(bool isOneTimeKeyBoard)
    {
        var buttons = Enum.GetValues(typeof(Exchange))
            .Cast<Exchange>()
            .Select(exchange => new KeyboardButton(exchange.ToString()))
            .ToList();

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons.Select(keyboardButton => new List<KeyboardButton> { keyboardButton }))
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = isOneTimeKeyBoard
        };

        return replyKeyboardMarkup;
    }

    public static InlineKeyboardMarkup GetSelectingAccountInlineKeyboard(List<AccountEntity> accounts)
    {
        var buttons = accounts
            .Select(account => InlineKeyboardButton.WithCallbackData($"{account.Exchange.Exchange.ToString()}-{account.Id.ToString()[..4]}", account.Id.ToString()))
            .Select(button => new[] { button })
            .ToList();

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

        return inlineKeyboard;
    }

}
