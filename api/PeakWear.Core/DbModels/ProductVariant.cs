using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(Sku), IsUnique = true)]
[Index(nameof(ProductId))]
public class ProductVariant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }

    [Required, StringLength(32)]
    public string Colour { get; set; } = "";

    // S, M, L
    [Required, StringLength(8)]
    public string Size { get; set; } = "";

    // Human-readable stock keeping unit, e.g. "LEG-BLK-M"
    [Required, StringLength(64)]
    public string Sku { get; set; } = "";

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}