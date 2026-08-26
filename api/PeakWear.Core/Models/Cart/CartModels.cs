namespace PeakWear.Core.Models.Cart;

public class AddToCartRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductSlug { get; set; } = "";
    public string Colour { get; set; } = "";
    public string Size { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int StockAvailable { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = [];
    public int ItemCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
}