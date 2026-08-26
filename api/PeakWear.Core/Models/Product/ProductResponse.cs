namespace PeakWear.Core.Models.Product;

public class ProductListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Colours { get; set; } = [];
}

public class ProductDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string Category { get; set; } = "";
    public decimal BasePrice { get; set; }
    public List<VariantResponse> Variants { get; set; } = [];
}

public class VariantResponse
{
    public Guid Id { get; set; }
    public string Colour { get; set; } = "";
    public string Size { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
    public bool InStock => Stock > 0;
}