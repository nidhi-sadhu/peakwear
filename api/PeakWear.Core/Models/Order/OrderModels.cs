using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.Models.Order;

public class PlaceOrderRequest
{
    [Required]
    public Guid AddressId { get; set; }
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "";
    public string Colour { get; set; } = "";
    public string Size { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public string ShippingAddress { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class PlaceOrderResult
{
    public OrderResponse? Order { get; set; }
    public string? Error { get; set; }
    public bool Success => Order is not null;
}