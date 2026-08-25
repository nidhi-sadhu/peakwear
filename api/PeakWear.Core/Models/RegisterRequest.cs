using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.Models;

public class RegisterRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = "";

    [Required, StringLength(64)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(64)]
    public string LastName { get; set; } = "";
}