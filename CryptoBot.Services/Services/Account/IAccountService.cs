using CryptoBot.Service.Models.Account;
using Microsoft.AspNetCore.JsonPatch;

namespace CryptoBot.Service.Services.Account
{
    public interface IAccountService
    {
        Task<AccountModel> GetSelectedAccountAsync(long chatId);

        Task DeleteSelectedAccountAsync(long chatId);

        Task SelectAccountAsync(long chatId, Guid newSelectedAccountId);

        Task<List<AccountModel>> GetAccountsAsync(long chatId, int limit, int shift = 0);

        Task CreateAccountAndSelectItAsync(long chatId);

        Task PatchSelectedAccountAsync(long chatId, JsonPatchDocument patchDocument);
    }
}
