using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PeakWear.Core.DbModels;
using PeakWear.Core.Models.Order;

namespace PeakWear.Core.Services;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentClient _payments;
    private readonly string _currency;
    private const decimal FreeShippingThreshold = 150m;
    private const decimal StandardShipping = 9.95m;

    public OrderService(IOrderRepository repository, IPaymentClient payments, IConfiguration config)
    {
        _repository = repository;
        _payments = payments;
        _currency = config["Stripe:Currency"] ?? "usd";
    }

    public async Task<PlaceOrderResult> PlaceOrderAsync(Guid userId, PlaceOrderRequest request)
    {
        var cart = await _repository.GetCartForCheckoutAsync(userId);
        if (cart.Count == 0)
            return new PlaceOrderResult { Error = "Your bag is empty." };

        var address = await _repository.GetAddressAsync(userId, request.AddressId);
        if (address is null)
            return new PlaceOrderResult { Error = "That address could not be found." };

        // Re-check stock now. It was checked when items went into the cart,
        // but that could have been days ago.
        foreach (var item in cart)
        {
            var variant = item.ProductVariant;
            if (variant is null)
                return new PlaceOrderResult { Error = "An item in your bag is no longer available." };

            if (variant.Stock < item.Quantity)
                return new PlaceOrderResult
                {
                    Error = $"{variant.Product?.Name} in {variant.Colour}, size {variant.Size} " +
                            $"only has {variant.Stock} left."
                };
        }

        var items = cart.Select(c =>
        {
            var unitPrice = c.ProductVariant!.Product!.BasePrice;
            return new OrderItem
            {
                ProductVariantId = c.ProductVariantId,
                ProductName = c.ProductVariant.Product.Name,
                Colour = c.ProductVariant.Colour,
                Size = c.ProductVariant.Size,
                Sku = c.ProductVariant.Sku,
                ImageUrl = c.ProductVariant.ImageUrl,
                UnitPrice = unitPrice,
                Quantity = c.Quantity,
                LineTotal = unitPrice * c.Quantity
            };
        }).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var shipping = subtotal >= FreeShippingThreshold ? 0m : StandardShipping;

        var order = new Order
        {
            OrderNumber = await NextOrderNumberAsync(),
            UserId = userId,
            Status = OrderStatus.Pending, // was "Paid" — the webhook decides now           
            Subtotal = subtotal,
            ShippingCost = shipping,
            Total = subtotal + shipping,
            ShipLine1 = address.Line1,
            ShipLine2 = address.Line2,
            ShipCity = address.City,
            ShipState = address.State,
            ShipPostalCode = address.PostalCode,
            ShipCountryCode = address.CountryCode,
            Items = items
        };

        var stockChanges = items
            .Select(i => (i.ProductVariantId, i.Quantity))
            .ToList();

        try
        {
            var placed = await _repository.PlaceOrderAsync(order, stockChanges, userId);
            // Stock is now reserved and the order exists. Only after that do we ask
            // Stripe for money — never inside the transaction, because a network call
            // to a third party would hold row locks open for the length of it.
            var email = await _repository.GetUserEmailAsync(userId);

            var intent = await _payments.CreateIntentAsync(
                Money.ToMinorUnits(placed.Total),
                _currency,
                placed.OrderNumber,
                email);

            await _repository.SetPaymentIntentAsync(placed.Id, intent.PaymentIntentId);

            return new PlaceOrderResult
            {
                Order = Map(placed),
                ClientSecret = intent.ClientSecret
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else bought the same variant between our stock check and our save
            return new PlaceOrderResult
            {
                Error = "Someone just bought the last one. Please review your bag and try again."
            };
        }
        catch (InvalidOperationException ex)
        {
            return new PlaceOrderResult { Error = ex.Message };
        }
    }

    public async Task<List<OrderResponse>> GetOrdersAsync(Guid userId) =>
        (await _repository.GetOrdersAsync(userId)).Select(Map).ToList();

    public async Task<OrderResponse?> GetOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _repository.GetOrderAsync(userId, orderId);
        return order is null ? null : Map(order);
    }

    private async Task<string> NextOrderNumberAsync() =>
        $"PW-{await _repository.NextOrderNumberAsync():D6}";

    private static OrderResponse Map(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status,
        Subtotal = o.Subtotal,
        ShippingCost = o.ShippingCost,
        Total = o.Total,
        ShippingAddress = string.Join(", ", new[]
        {
            o.ShipLine1, o.ShipLine2, o.ShipCity,
            $"{o.ShipState} {o.ShipPostalCode}", o.ShipCountryCode
        }.Where(s => !string.IsNullOrWhiteSpace(s))),
        CreatedAtUtc = o.CreatedAtUtc,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            Id = i.Id,
            ProductName = i.ProductName,
            Colour = i.Colour,
            Size = i.Size,
            Sku = i.Sku,
            ImageUrl = i.ImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList()
    };
}