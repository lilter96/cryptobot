using CryptoBot.Data.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoBot.TelegramBot.Keyboards;

public static class TelegramKeyboards
{
    public static ReplyKeyboardMarkup GetDefaultKeyboard()
    {
        var buttons = new List<List<KeyboardButton>>
        {
            new()
            {
                new KeyboardButton("Аккаунт"),
                new KeyboardButton("Команды")
            }
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
        var buttons = new List<List<KeyboardButton>>
        {
            new()
            {
                new KeyboardButton("Bybit"),
                new KeyboardButton("Binance")
            }
        };

        var replyKeyboardMarkup = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = isOneTimeKeyBoard
        };

        return replyKeyboardMarkup;
    }
    
    public static InlineKeyboardMarkup GetSelectingAccountInlineKeyboard(List<AccountEntity> accounts)
    {
        var buttons = accounts
            .Select(account => InlineKeyboardButton.WithCallbackData($"{account.Id}", account.Id.ToString()))
            .Select(button => new[] { button })
            .ToList();

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);
    
        return inlineKeyboard;
    }

}
