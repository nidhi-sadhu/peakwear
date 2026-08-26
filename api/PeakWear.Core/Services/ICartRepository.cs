using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetByUserAsync(Guid userId);
    Task<CartItem?> FindAsync(Guid userId, Guid variantId);
    Task<ProductVariant?> GetVariantAsync(Guid variantId);
    Task AddAsync(CartItem item);
    Task UpdateAsync(CartItem item);
    Task RemoveAsync(CartItem item);
    Task ClearAsync(Guid userId);
}