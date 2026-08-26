using PeakWear.Core.Models.Product;

namespace PeakWear.Core.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository) =>
        _productRepository = productRepository;

    public async Task<IEnumerable<ProductListItem>> GetByCategoryAsync(string category)
    {
        var products = category.Equals("new", StringComparison.OrdinalIgnoreCase)
            ? await _productRepository.GetNewestAsync(8)
            : await _productRepository.GetByShoppingForAsync(Normalise(category));

        return products.Select(p => new ProductListItem
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Category = p.Category,
            BasePrice = p.BasePrice,
            ImageUrl = p.Variants.FirstOrDefault()?.ImageUrl,
            Colours = p.Variants.Select(v => v.Colour).Distinct().ToList()
        });
    }

    public async Task<ProductDetail?> GetBySlugAsync(string slug)
    {
        var p = await _productRepository.GetBySlugAsync(slug);
        if (p is null) return null;

        return new ProductDetail
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            Category = p.Category,
            BasePrice = p.BasePrice,
            Variants = p.Variants
                .OrderBy(v => v.Colour).ThenBy(v => v.Size)
                .Select(v => new VariantResponse
                {
                    Id = v.Id,
                    Colour = v.Colour,
                    Size = v.Size,
                    Sku = v.Sku,
                    ImageUrl = v.ImageUrl,
                    Stock = v.Stock
                }).ToList()
        };
    }

    private static string Normalise(string category) =>
        category.Equals("women", StringComparison.OrdinalIgnoreCase) ? "Women" : "Men";
}