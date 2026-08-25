using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(UserId), IsUnique = true)]
public class UserPreference
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required, StringLength(16)]
    public string ShoppingFor { get; set; } = "Both";   // Men, Women, Both

    [StringLength(8)]
    public string? SizeTop { get; set; }

    [StringLength(8)]
    public string? SizeBottom { get; set; }

    public bool MarketingOptIn { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}