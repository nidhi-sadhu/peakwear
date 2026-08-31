using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface IOrderRepository
{
    Task<List<CartItem>> GetCartForCheckoutAsync(Guid userId);
    Task<Address?> GetAddressAsync(Guid userId, Guid addressId);
    Task<ProductVariant?> GetVariantForUpdateAsync(Guid variantId);
    Task<int> CountOrdersAsync();
    Task<Order> PlaceOrderAsync(Order order, List<(Guid VariantId, int Quantity)> stockChanges, Guid userId);
    Task<List<Order>> GetOrdersAsync(Guid userId);
    Task<Order?> GetOrderAsync(Guid userId, Guid orderId);
}