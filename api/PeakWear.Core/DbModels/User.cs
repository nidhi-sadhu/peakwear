using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeakWear.Core.DbModels;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(256)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, StringLength(64)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(64)]
    public string LastName { get; set; } = "";

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required, StringLength(32)]
    public string Role { get; set; } = "Customer";

    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }

    public DateTime? LastLoginAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public uint Version { get; set; }

    public UserPreference? Preference { get; set; }
    public List<Address> Addresses { get; set; } = [];
}