using CryptoBot.Data;
using CryptoBot.Data.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.Service.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly CryptoBotDbContext _dbContext;

        public ChatService(CryptoBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateChatAsync(long chatId)
        {
            var isChatNotExist = await _dbContext.Chats.AllAsync(x => x.Id != chatId);

            if (!isChatNotExist)
            {
                return;
            }

            _dbContext.Add(new ChatEntity
            {
                BotState = BotState.WaitingForCommand,
                Accounts = [],
                Id = chatId
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetBotStateAsync(long chatId)
        {
            var chat = await _dbContext.Chats.SingleAsync(x => x.Id == chatId);

            return (int) chat.BotState;
        }

        public async Task PatchChatAsync(long chatId, JsonPatchDocument patchDocument)
        {
            var chat = await _dbContext.Chats
                .FirstOrDefaultAsync(x => x.Id == chatId);

            try
            {
                patchDocument.ApplyTo(chat);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid patch document for Chat", ex);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
