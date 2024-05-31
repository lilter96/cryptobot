namespace CryptoBot.Data.Entities
{
    public class ExchangeEntity : IEntity
    {
        public Guid Id { get; init; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModificationDate { get; set; }

        public Exchange Exchange { get; set; }

        public string EncryptedKey { get; set; }

        public string EncryptedSecret { get; set; }

        public Guid AccountId { get; set; }

        public virtual AccountEntity Account { get; set; }
    }
}
