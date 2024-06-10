namespace CryptoBot.Service.Models.Account
{
    public class AccountModel
    {
        public Guid Id { get; set; }

        public string Exchange { get; set; }

        public long ChatId { get; set; }

        public string EncryptedApiKey { get; set; }

        public string EncryptedApiSecret { get; set; }

        public string EncryptedApiPassPhrase { get; set; }
    }
}
