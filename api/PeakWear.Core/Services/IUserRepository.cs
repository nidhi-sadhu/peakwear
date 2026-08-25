using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateLastLoginAsync(Guid userId);
}