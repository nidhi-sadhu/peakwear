using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
}