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

    // Only what attributes can't express: SQL-level behaviour, relationships,
    // decimal precision and partial indexes.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- User ----------
        modelBuilder.Entity<User>(entity =>
        {
            // Postgres keeps a hidden xmin column on every row holding the transaction
            // ID that last wrote it. Mapping it as a concurrency token makes EF add
            // "WHERE xmin = <value read earlier>" to every UPDATE, so a concurrent
            // write throws DbUpdateConcurrencyException instead of silently winning.
            entity.Property(u => u.Version)
                  .HasColumnName("xmin")
                  .HasColumnType("xid")
                  .ValueGeneratedOnAddOrUpdate()
                  .IsConcurrencyToken();

            // Deleting a user removes their preference, addresses and cart
            entity.HasOne(u => u.Preference)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserPreference>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Addresses)
                  .WithOne(a => a.User)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Address ----------
        modelBuilder.Entity<Address>(entity =>
        {
            // Lookup index — "all addresses for this user"
            entity.HasIndex(a => a.UserId)
                  .HasDatabaseName("ix_addresses_user_id");

            // Partial unique index: unique on user_id, but only across rows where
            // is_default is true. Guarantees one default per user at the database
            // level. No data-annotation equivalent exists for this.
            entity.HasIndex(a => a.UserId)
                  .IsUnique()
                  .HasFilter("is_default")
                  .HasDatabaseName("ix_addresses_user_default");
        });

        // ---------- Product ----------
        modelBuilder.Entity<Product>(entity =>
        {
            // Money needs explicit precision — the provider default can be lossy
            entity.Property(p => p.BasePrice).HasPrecision(18, 2);

            entity.HasMany(p => p.Variants)
                  .WithOne(v => v.Product)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- ProductVariant ----------
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            // A product can't have two variants of the same colour and size
            entity.HasIndex(v => new { v.ProductId, v.Colour, v.Size })
                  .IsUnique()
                  .HasDatabaseName("ix_variants_product_colour_size");

            // Optimistic concurrency on stock. If two checkouts race for the last
            // item, the second one's UPDATE matches zero rows and EF throws.
            entity.Property(v => v.Version)
                  .HasColumnName("xmin")
                  .HasColumnType("xid")
                  .ValueGeneratedOnAddOrUpdate()
                  .IsConcurrencyToken();
        });

        // ---------- CartItem ----------
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // A discontinued variant should disappear from carts
            entity.HasOne(c => c.ProductVariant)
                  .WithMany()
                  .HasForeignKey(c => c.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Order ----------
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.ShippingCost).HasPrecision(18, 2);
            entity.Property(o => o.Total).HasPrecision(18, 2);

            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Restrict, not Cascade — deleting a user must not silently erase
            // financial records. The delete fails and forces an explicit decision.
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- OrderItem ----------
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.LineTotal).HasPrecision(18, 2);

            // Referential integrity on the variant, but Restrict so a sold item
            // can't be deleted out from under its order history. Products are
            // retired by setting is_active = false, not deleted.
            entity.HasOne<ProductVariant>()
                  .WithMany()
                  .HasForeignKey(i => i.ProductVariantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}