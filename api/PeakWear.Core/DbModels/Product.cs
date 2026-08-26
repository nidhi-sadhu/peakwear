using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(Slug), IsUnique = true)]
[Index(nameof(Category))]
[Index(nameof(ShoppingFor))]
public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(220)]
    public string Slug { get; set; } = "";

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, StringLength(32)]
    public string Category { get; set; } = "";

    // Men or Women
    [Required, StringLength(16)]
    public string ShoppingFor { get; set; } = "";

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<ProductVariant> Variants { get; set; } = [];
}