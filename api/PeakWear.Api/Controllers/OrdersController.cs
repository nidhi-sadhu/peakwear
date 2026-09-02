using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Models.Order;
using PeakWear.Core.Services;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService) => _orderService = orderService;

    private Guid UserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request)
    {
        var result = await _orderService.PlaceOrderAsync(UserId, request);
        return result.Success
            ? Ok(new CheckoutResponse
            {
                Order = result.Order!,
                ClientSecret = result.ClientSecret!
            })
            : Conflict(new { message = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders() =>
        Ok(await _orderService.GetOrdersAsync(UserId));

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var order = await _orderService.GetOrderAsync(UserId, orderId);
        return order is null ? NotFound() : Ok(order);
    }
}