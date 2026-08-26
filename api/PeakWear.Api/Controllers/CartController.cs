using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Models.Cart;
using PeakWear.Core.Services;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]                       // whole controller requires a valid token
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService) => _cartService = cartService;

    // The user id comes from the token, never from the request body —
    // otherwise anyone could read or edit someone else's cart.
    private Guid UserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _cartService.GetAsync(UserId));

    [HttpPost]
    public async Task<IActionResult> Add(AddToCartRequest request)
    {
        var cart = await _cartService.AddAsync(UserId, request);
        return cart is null
            ? BadRequest(new { message = "That item isn't available in the quantity requested." })
            : Ok(cart);
    }

    [HttpPut("{itemId:guid}")]
    public async Task<IActionResult> UpdateQuantity(Guid itemId, UpdateQuantityRequest request)
    {
        var cart = await _cartService.UpdateQuantityAsync(UserId, itemId, request.Quantity);
        return cart is null
            ? BadRequest(new { message = "Could not update that item." })
            : Ok(cart);
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Remove(Guid itemId)
    {
        var cart = await _cartService.RemoveAsync(UserId, itemId);
        return cart is null ? NotFound() : Ok(cart);
    }
}