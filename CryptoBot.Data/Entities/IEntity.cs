namespace CryptoBot.Data.Entities;

public interface IEntity<T>
{
    public T Id { get; init; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime ModificationDate { get; set; }
}