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

    Task<long> NextOrderNumberAsync();
    Task<string?> GetUserEmailAsync(Guid userId);
    Task SetPaymentIntentAsync(Guid orderId, string paymentIntentId);

    Task<bool> TryRecordEventAsync(string eventId, string eventType);
    Task<Order?> GetByPaymentIntentAsync(string paymentIntentId);
    Task MarkPaidAsync(Guid orderId);
    Task MarkFailedAndRestoreStockAsync(Guid orderId);
    Task<List<Order>> GetStalePendingOrdersAsync(DateTime olderThanUtc);
    Task ExpireAndRestoreStockAsync(Guid orderId);
}