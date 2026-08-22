using PeakWear.Core.Models.User;

namespace PeakWear.Core.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName
        });
    }
}