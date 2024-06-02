using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoBot.Data.Entities;

public class ChatEntity : IEntity<long>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; init; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public BotState BotState { get; set; }

    public Guid? SelectedAccountId { get; set; }

    public virtual IList<AccountEntity> Accounts { get; init; }

    public virtual AccountEntity SelectedAccount { get; set; }
}
