using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(UserId))]
public class Address
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required, StringLength(100)]
    public string Line1 { get; set; } = "";

    [StringLength(100)]
    public string? Line2 { get; set; }

    [Required, StringLength(64)]
    public string City { get; set; } = "";

    [Required, StringLength(64)]
    public string State { get; set; } = "";

    [Required, StringLength(16)]
    public string PostalCode { get; set; } = "";

    [Required, StringLength(2)]
    public string CountryCode { get; set; } = "US";

    public bool IsDefault { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}