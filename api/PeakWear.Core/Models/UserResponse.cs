namespace PeakWear.Core.Models.User;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
}