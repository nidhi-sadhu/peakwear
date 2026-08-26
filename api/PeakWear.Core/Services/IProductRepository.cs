using PeakWear.Core.DbModels;

namespace PeakWear.Core.Services;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetByShoppingForAsync(string shoppingFor);
    Task<IEnumerable<Product>> GetNewestAsync(int count);
    Task<Product?> GetBySlugAsync(string slug);
}