namespace PeakWear.Core.Models;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponse User { get; set; } = new();
}