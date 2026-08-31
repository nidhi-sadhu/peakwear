using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(OrderId))]
public class OrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    // Kept for reference, but everything needed to display the line is copied below
    public Guid ProductVariantId { get; set; }

    // Snapshot at purchase time. If the product is renamed, repriced or deleted,
    // the order still shows what was actually bought and what was actually paid.
    [Required, StringLength(200)] public string ProductName { get; set; } = "";
    [Required, StringLength(32)]  public string Colour { get; set; } = "";
    [Required, StringLength(8)]   public string Size { get; set; } = "";
    [Required, StringLength(64)]  public string Sku { get; set; } = "";
    [StringLength(500)]           public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
}