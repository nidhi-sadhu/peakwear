using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;

namespace PeakWear.Data;

public class PeakWearDbContext : DbContext
{
    public PeakWearDbContext(DbContextOptions<PeakWearDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<Address> Addresses => Set<Address>();

    // Only what attributes can't express: SQL-level behaviour and cascade rules.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Postgres' hidden xmin column — optimistic concurrency, no extra column needed
        modelBuilder.Entity<User>()
            .Property(u => u.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Deleting a user removes their preference and addresses too
        modelBuilder.Entity<User>()
            .HasOne(u => u.Preference)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Partial unique index — only one default address per user. No attribute for this.
        modelBuilder.Entity<Address>()
            .HasIndex(a => a.UserId)
            .IsUnique()
            .HasFilter("is_default")
            .HasDatabaseName("ix_addresses_user_default");
    }
}