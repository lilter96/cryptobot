namespace CryptoBot.Data.Entities;

public class AccountEntity : IEntity<Guid>
{
    public Guid Id { get; init; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public long ChatId { get; init; }

    public ChatEntity Chat { get; init; }

    public Guid ExchangeId { get; set; }

    public ExchangeEntity Exchange { get; set; }
}
