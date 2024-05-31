using CryptoBot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.Data;

public class CryptoBotDbContext : DbContext
{
    public virtual DbSet<AccountEntity> Accounts { get; init; }

    public virtual DbSet<ChatEntity> Chats { get; init; }

    public virtual DbSet<ExchangeEntity> Exchanges { get; init; }

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

        modelBuilder.Entity<ExchangeEntity>()
            .HasOne(x => x.Account)
            .WithOne(x => x.Exchange)
            .HasForeignKey<AccountEntity>(x => x.ExchangeId);
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
            if (entry is not IEntity entity)
            {
                continue;
            }

            entity.CreatedDate = DateTime.UtcNow;
            entity.ModificationDate = DateTime.UtcNow;
        }

        var updated = ChangeTracker
            .Entries()
            .Where(w => w.State == EntityState.Modified)
            .Select(s => s.Entity)
            .ToList();

        foreach (var entry in updated)
        {
            if (entry is IEntity entity)
            {
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
