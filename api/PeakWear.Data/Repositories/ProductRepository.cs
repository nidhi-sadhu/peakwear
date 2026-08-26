using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PeakWearDbContext _context;

    public ProductRepository(PeakWearDbContext context) => _context = context;

    public async Task<IEnumerable<Product>> GetByShoppingForAsync(string shoppingFor) =>
        await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Where(p => p.IsActive && p.ShoppingFor == shoppingFor)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetNewestAsync(int count) =>
        await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(count)
            .ToListAsync();

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.IsActive && p.Slug == slug);
}