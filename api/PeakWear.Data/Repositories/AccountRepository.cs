using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly PeakWearDbContext _context;

    public AccountRepository(PeakWearDbContext context) => _context = context;

    public async Task<User?> GetProfileAsync(Guid userId) =>
        await _context.Users
            .AsNoTracking()
            .Include(u => u.Preference)
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

    // Tracked, so EF can generate the UPDATE on SaveAsync
    public async Task<User?> GetForUpdateAsync(Guid userId) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

    public async Task SaveAsync() => await _context.SaveChangesAsync();

    public async Task<UserPreference?> GetPreferenceAsync(Guid userId) =>
        await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task AddPreferenceAsync(UserPreference preference)
    {
        _context.UserPreferences.Add(preference);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Address>> GetAddressesAsync(Guid userId) =>
        await _context.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAtUtc)
            .ToListAsync();

    public async Task<Address?> GetAddressAsync(Guid userId, Guid addressId) =>
        await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

    public async Task AddAddressAsync(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAddressAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }
    
    public async Task ClearDefaultAddressesAsync(Guid userId) =>
        await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
}