using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.DbModels;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(256)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, StringLength(128)]
    public string DisplayName { get; set; } = "";

    [Required, StringLength(32)]
    public string Role { get; set; } = "Customer";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}