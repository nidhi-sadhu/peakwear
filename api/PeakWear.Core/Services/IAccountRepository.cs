using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface IAccountRepository
{
    Task<User?> GetProfileAsync(Guid userId);
    Task<User?> GetForUpdateAsync(Guid userId);
    Task SaveAsync();

    Task<UserPreference?> GetPreferenceAsync(Guid userId);
    Task AddPreferenceAsync(UserPreference preference);

    Task<List<Address>> GetAddressesAsync(Guid userId);
    Task<Address?> GetAddressAsync(Guid userId, Guid addressId);
    Task AddAddressAsync(Address address);
    Task RemoveAddressAsync(Address address);
    Task ClearDefaultAddressesAsync(Guid userId);
}