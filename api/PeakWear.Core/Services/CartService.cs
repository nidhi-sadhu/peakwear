using PeakWear.Core.DbModels;
using PeakWear.Core.Models.Cart;

namespace PeakWear.Core.Services;

public class CartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository) => _cartRepository = cartRepository;

    public async Task<CartResponse> GetAsync(Guid userId)
    {
        var items = await _cartRepository.GetByUserAsync(userId);

        return new CartResponse
        {
            Items = items.Select(c => new CartItemResponse
            {
                Id = c.Id,
                ProductVariantId = c.ProductVariantId,
                ProductName = c.ProductVariant?.Product?.Name ?? "",
                ProductSlug = c.ProductVariant?.Product?.Slug ?? "",
                Colour = c.ProductVariant?.Colour ?? "",
                Size = c.ProductVariant?.Size ?? "",
                Sku = c.ProductVariant?.Sku ?? "",
                ImageUrl = c.ProductVariant?.ImageUrl,
                UnitPrice = c.ProductVariant?.Product?.BasePrice ?? 0,
                Quantity = c.Quantity,
                StockAvailable = c.ProductVariant?.Stock ?? 0
            }).ToList()
        };
    }

    // Returns null when the variant doesn't exist or there isn't enough stock
    public async Task<CartResponse?> AddAsync(Guid userId, AddToCartRequest request)
    {
        var variant = await _cartRepository.GetVariantAsync(request.ProductVariantId);
        if (variant is null) return null;

        var existing = await _cartRepository.FindAsync(userId, request.ProductVariantId);
        var newQuantity = (existing?.Quantity ?? 0) + request.Quantity;

        // Stock is checked against the total, not just the amount being added
        if (newQuantity > variant.Stock) return null;

        if (existing is null)
        {
            await _cartRepository.AddAsync(new CartItem
            {
                UserId = userId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity
            });
        }
        else
        {
            existing.Quantity = newQuantity;
            await _cartRepository.UpdateAsync(existing);
        }

        return await GetAsync(userId);
    }

    public async Task<CartResponse?> UpdateQuantityAsync(Guid userId, Guid itemId, int quantity)
    {
        var items = await _cartRepository.GetByUserAsync(userId);
        var target = items.FirstOrDefault(i => i.Id == itemId);
        if (target is null) return null;

        var tracked = await _cartRepository.FindAsync(userId, target.ProductVariantId);
        if (tracked is null) return null;

        if (quantity <= 0)
        {
            await _cartRepository.RemoveAsync(tracked);
            return await GetAsync(userId);
        }

        var variant = await _cartRepository.GetVariantAsync(tracked.ProductVariantId);
        if (variant is null || quantity > variant.Stock) return null;

        tracked.Quantity = quantity;
        await _cartRepository.UpdateAsync(tracked);
        return await GetAsync(userId);
    }

    public async Task<CartResponse?> RemoveAsync(Guid userId, Guid itemId)
    {
        var items = await _cartRepository.GetByUserAsync(userId);
        var target = items.FirstOrDefault(i => i.Id == itemId);
        if (target is null) return null;

        var tracked = await _cartRepository.FindAsync(userId, target.ProductVariantId);
        if (tracked is not null) await _cartRepository.RemoveAsync(tracked);

        return await GetAsync(userId);
    }
}