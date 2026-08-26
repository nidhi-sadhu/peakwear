using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

// One row per user + variant. Adding the same variant twice bumps quantity.
[Index(nameof(UserId), nameof(ProductVariantId), IsUnique = true)]
public class CartItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid ProductVariantId { get; set; }

    public int Quantity { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}