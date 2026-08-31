using Microsoft.EntityFrameworkCore;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PeakWearDbContext _context;

    public OrderRepository(PeakWearDbContext context) => _context = context;

    public async Task<List<CartItem>> GetCartForCheckoutAsync(Guid userId) =>
        await _context.CartItems
            .AsNoTracking()
            .Include(c => c.ProductVariant)!
                .ThenInclude(v => v!.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

    public async Task<Address?> GetAddressAsync(Guid userId, Guid addressId) =>
        await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

    // Tracked, so the concurrency token is loaded and checked on save
    public async Task<ProductVariant?> GetVariantForUpdateAsync(Guid variantId) =>
        await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);

    public async Task<int> CountOrdersAsync() => await _context.Orders.CountAsync();

    public async Task<Order> PlaceOrderAsync(
        Order order,
        List<(Guid VariantId, int Quantity)> stockChanges,
        Guid userId)
    {
        // Everything below commits together or not at all: stock decrement,
        // order creation, and clearing the cart.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var (variantId, quantity) in stockChanges)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == variantId)
                    ?? throw new InvalidOperationException($"Variant {variantId} no longer exists.");

                if (variant.Stock < quantity)
                    throw new InvalidOperationException($"{variant.Sku} is out of stock.");

                variant.Stock -= quantity;
                variant.UpdatedAtUtc = DateTime.UtcNow;
            }

            _context.Orders.Add(order);

            await _context.CartItems
                .Where(c => c.UserId == userId)
                .ExecuteDeleteAsync();

            // If another checkout changed the same variant since we read it,
            // xmin no longer matches and this throws DbUpdateConcurrencyException.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Order>> GetOrdersAsync(Guid userId) =>
        await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

    public async Task<Order?> GetOrderAsync(Guid userId, Guid orderId) =>
        await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
}