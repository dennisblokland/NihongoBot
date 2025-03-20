using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using NihongoBot.Domain.Base;

namespace NihongoBot.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext() : base() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=NihongoBot;Username=dev;Password=devpassword");
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
         modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        IEnumerable<EntityEntry<DomainEntity>> entries = ChangeTracker.Entries<DomainEntity>();

        foreach (EntityEntry<DomainEntity> entry in entries)
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                entry.Entity.UpdateTimestamps();
            }
        }
    }
}
