using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _config;

    public PaymentsController(IConfiguration config) => _config = config;
    
    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetConfig() =>
        Ok(new { publishableKey = _config["Stripe:PublishableKey"] });
}