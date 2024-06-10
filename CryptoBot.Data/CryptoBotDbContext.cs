using CryptoBot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.Data;

public class CryptoBotDbContext : DbContext
{
    public virtual DbSet<AccountEntity> Accounts { get; init; }

    public virtual DbSet<ChatEntity> Chats { get; init; }

    public CryptoBotDbContext(DbContextOptions<CryptoBotDbContext> opt) : base(opt) { }

    public CryptoBotDbContext() { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatEntity>()
            .HasMany(c => c.Accounts)
            .WithOne(a => a.Chat)
            .HasForeignKey(a => a.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatEntity>()
            .HasOne(c => c.SelectedAccount)
            .WithOne()
            .HasForeignKey<ChatEntity>(c => c.SelectedAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges()
    {
        ChangeTracker.DetectChanges();

        var added = ChangeTracker
            .Entries()
            .Where(w => w.State == EntityState.Added)
            .Select(s => s.Entity)
            .ToList();

        foreach (var entry in added)
        {
            var entityType = entry.GetType();
            var entityInterfaces = entityType.GetInterfaces();

            var entityInterface = entityInterfaces.FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

            if (entityInterface != null)
            {
                dynamic entity = entry;
                entity.CreatedDate = DateTime.UtcNow;
                entity.ModificationDate = DateTime.UtcNow;
            }
        }

        var updated = ChangeTracker
            .Entries()
            .Where(w => w.State == EntityState.Modified)
            .Select(s => s.Entity)
            .ToList();

        foreach (var entry in updated)
        {
            var entityType = entry.GetType();
            var entityInterfaces = entityType.GetInterfaces();

            var entityInterface = entityInterfaces.FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

            if (entityInterface != null)
            {
                dynamic entity = entry;
                entity.ModificationDate = DateTime.UtcNow;
            }
        }

        return base.SaveChanges();
    }


    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        return Task.Run(SaveChanges, cancellationToken);
    }
}
