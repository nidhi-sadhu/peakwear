using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;

namespace PeakWear.Data;

public class PeakWearDbContext : DbContext
{
    public PeakWearDbContext(DbContextOptions<PeakWearDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // things EF can't guess:
            entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(u => u.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(p => p.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}