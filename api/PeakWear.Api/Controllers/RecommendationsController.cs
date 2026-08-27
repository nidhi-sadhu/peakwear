using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Models.Recommendation;
using PeakWear.Core.Services;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly SizeRecommendationService _service;

    public RecommendationsController(SizeRecommendationService service) => _service = service;

    [HttpPost("size")]
    public async Task<IActionResult> RecommendSize(
        SizeRecommendationRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.RecommendAsync(request, cancellationToken);
        return result is null ? NotFound(new { message = "Product not found." }) : Ok(result);
    }
}