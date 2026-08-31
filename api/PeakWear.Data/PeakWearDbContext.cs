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
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

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

        // Money needs explicit precision — the default can be lossy
        modelBuilder.Entity<Product>()
            .Property(p => p.BasePrice)
            .HasPrecision(18, 2);

        // Deleting a product removes its variants
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // A product can't have two variants of the same colour and size
        modelBuilder.Entity<ProductVariant>()
            .HasIndex(v => new { v.ProductId, v.Colour, v.Size })
            .IsUnique()
            .HasDatabaseName("ix_variants_product_colour_size");

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ProductVariant)
                    .WithMany()
                    .HasForeignKey(c => c.ProductVariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Subtotal).HasPrecision(18, 2);
                entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
                entity.Property(o => o.Total).HasPrecision(18, 2);

                entity.HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.LineTotal).HasPrecision(18, 2);
            });

            // Optimistic concurrency on stock. If two checkouts race for the last item,
            // the second one's UPDATE matches zero rows and EF throws.
            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
    }
}