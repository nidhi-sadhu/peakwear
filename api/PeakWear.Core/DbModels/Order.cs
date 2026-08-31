using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(OrderNumber), IsUnique = true)]
[Index(nameof(UserId))]
public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Human-readable, for customer support. GUIDs are unreadable over the phone.
    [Required, StringLength(20)]
    public string OrderNumber { get; set; } = "";

    public Guid UserId { get; set; }

    // Pending, Paid, Shipped, Cancelled
    [Required, StringLength(24)]
    public string Status { get; set; } = "Pending";

    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }

    // Address copied, not referenced — if the customer edits or deletes the
    // address later, the order must still show where it was actually sent.
    [Required, StringLength(100)] public string ShipLine1 { get; set; } = "";
    [StringLength(100)]           public string? ShipLine2 { get; set; }
    [Required, StringLength(64)]  public string ShipCity { get; set; } = "";
    [Required, StringLength(64)]  public string ShipState { get; set; } = "";
    [Required, StringLength(16)]  public string ShipPostalCode { get; set; } = "";
    [Required, StringLength(2)]   public string ShipCountryCode { get; set; } = "US";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = [];
}