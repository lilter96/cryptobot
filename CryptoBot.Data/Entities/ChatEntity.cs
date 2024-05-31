namespace CryptoBot.Data.Entities;

public class ChatEntity : IEntity
{
    public Guid Id { get; init; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public long ChatId { get; init; }

    public BotState BotState { get; set; }

    public Guid? SelectedAccountId { get; set; }

    public virtual IList<AccountEntity> Accounts { get; init; }

    public virtual AccountEntity SelectedAccount { get; set; }
}
