using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Services;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService) =>
        _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetByCategory([FromQuery] string category = "women") =>
        Ok(await _productService.GetByCategoryAsync(category));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        return product is null ? NotFound() : Ok(product);
    }
}