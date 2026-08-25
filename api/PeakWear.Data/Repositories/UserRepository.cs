using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PeakWearDbContext _context;

    public UserRepository(PeakWearDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users
            .AnyAsync(u => u.Email == email.ToLower());

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;                   
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return;

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}