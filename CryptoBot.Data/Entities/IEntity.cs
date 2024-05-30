namespace CryptoBot.Data.Entities;

public interface IEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModificationDate { get; set; }
}