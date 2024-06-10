using Microsoft.AspNetCore.JsonPatch;

namespace CryptoBot.Service.Services.Chat
{
    public interface IChatService
    {
        Task CreateChatAsync(long chatId);

        Task<int> GetBotStateAsync(long chatId);

        Task PatchChatAsync(long chatId, JsonPatchDocument patchDocument);
    }
}
