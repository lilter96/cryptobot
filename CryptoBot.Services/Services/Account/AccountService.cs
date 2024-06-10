using CryptoBot.Data;
using CryptoBot.Data.Entities;
using CryptoBot.Service.Models.Account;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.Service.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly CryptoBotDbContext _dbContext;
        public AccountService(
            CryptoBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AccountModel> GetSelectedAccountAsync(long chatId)
        {
            var chat = await _dbContext.Chats.Include(x => x.SelectedAccount).FirstOrDefaultAsync(x => x.Id == chatId);

            var selectedAccount = chat.SelectedAccount;

            return new AccountModel
            {
                Id = selectedAccount.Id,
                ChatId = selectedAccount.ChatId,
                EncryptedApiKey = selectedAccount.EncryptedApiKey,
                EncryptedApiSecret = selectedAccount.EncryptedApiSecret,
                Exchange = selectedAccount.Exchange.ToString(),
                EncryptedApiPassPhrase = selectedAccount.EncryptedApiPassPhrase
            };
        }

        public async Task DeleteSelectedAccountAsync(long chatId)
        {
            var chat = await _dbContext.Chats.Include(x => x.SelectedAccount).FirstOrDefaultAsync(x => x.Id == chatId);

            var selectedAccount = chat.SelectedAccount;

            _dbContext.Accounts.Remove(selectedAccount);

            await _dbContext.SaveChangesAsync();
        }

        public async Task SelectAccountAsync(long chatId, Guid newSelectedAccountId)
        {
            var account = await _dbContext.Accounts
                .Include(accountEntity => accountEntity.Chat)
                .Include(accountEntity => accountEntity.Exchange)
                .FirstOrDefaultAsync(x =>
                    x.Id == newSelectedAccountId &&
                    x.ChatId == chatId);

            account.Chat.SelectedAccountId = account.Id;
            _dbContext.Update(account.Chat);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<AccountModel>> GetAccountsAsync(long chatId, int limit, int shift = 0)
        {
            var accounts = await _dbContext.Accounts
                .Where(x => x.ChatId == chatId)
                .OrderBy(x => x.CreatedDate)
                .Skip(shift)
                .Take(limit)
                .Select(x => new AccountModel
                {
                    Id = x.Id,
                    ChatId = x.ChatId,
                    EncryptedApiKey = x.EncryptedApiKey,
                    EncryptedApiSecret = x.EncryptedApiSecret,
                    Exchange = x.Exchange.ToString(),
                    EncryptedApiPassPhrase = x.EncryptedApiPassPhrase
                })
                .ToListAsync();

            return accounts;
        }

        public async Task CreateAccountAndSelectItAsync(long chatId)
        {
            var account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
            };

            var chat = await _dbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId);

            chat.SelectedAccountId = account.Id;

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task PatchSelectedAccountAsync(long chatId, JsonPatchDocument patchDocument)
        {
            var chat = await _dbContext.Chats
                .Include(x => x.SelectedAccount)
                .FirstOrDefaultAsync(x => x.Id == chatId);

            try
            {
                patchDocument.ApplyTo(chat.SelectedAccount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid patch document", ex);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
