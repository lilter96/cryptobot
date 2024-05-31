using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptoBot.TelegramBot
{
    public static class UpdateExtension
    {
        public static long GetChatId(this Update update) =>
            update.Type switch
            {
                UpdateType.Message => update.Message!.Chat.Id,
                UpdateType.EditedMessage => update.EditedMessage!.Chat.Id,
                UpdateType.CallbackQuery => update.CallbackQuery!.Message!.Chat!.Id,
                UpdateType.ChannelPost => update.ChannelPost!.Chat!.Id,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat!.Id,
                UpdateType.MyChatMember => update.MyChatMember!.Chat.Id,
                UpdateType.ChatMember => update.ChatMember!.Chat.Id,
                UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat.Id,
                _ => throw new InvalidOperationException("Unsupported Update type to extrcat ChatId")
            };
    }
}
