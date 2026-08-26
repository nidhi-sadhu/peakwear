using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly PeakWearDbContext _context;

    public CartRepository(PeakWearDbContext context) => _context = context;

    // Two Includes deep: cart item -> variant -> product, so we can show names and prices
    public async Task<IEnumerable<CartItem>> GetByUserAsync(Guid userId) =>
        await _context.CartItems
            .AsNoTracking()
            .Include(c => c.ProductVariant)!
                .ThenInclude(v => v!.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync();

    public async Task<CartItem?> FindAsync(Guid userId, Guid variantId) =>
        await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductVariantId == variantId);

    public async Task<ProductVariant?> GetVariantAsync(Guid variantId) =>
        await _context.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId);

    public async Task AddAsync(CartItem item)
    {
        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CartItem item)
    {
        item.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAsync(Guid userId)
    {
        await _context.CartItems.Where(c => c.UserId == userId).ExecuteDeleteAsync();
    }
}