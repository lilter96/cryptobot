using CryptoBot.TelegramBot;

namespace CryptoBot.Data.Entities;

public class ChatEntity : IEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModificationDate { get; set; }
    
    public Guid ChatId { get; init; }
    
    public BotCommand LastCommand { get; init; }
    
    public virtual IList<AccountEntity> Accounts { get; init; }
}