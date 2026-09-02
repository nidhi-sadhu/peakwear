using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWear.Core.Services;
using Stripe;

namespace PeakWear.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly IConfiguration _config;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IOrderRepository repository,
        IConfiguration config,
        ILogger<WebhooksController> logger)
    {
        _repository = repository;
        _config = config;
        _logger = logger;
    }

    // AllowAnonymous because Stripe has no JWT. The signature check below is
    // what makes this safe — without it, anyone could POST "order paid" here.
    [HttpPost("stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> Stripe()
    {
        // Read the raw body. Stripe signs the exact bytes it sent, so a
        // deserialized-then-reserialized object would fail verification.
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _config["Stripe:WebhookSecret"]);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Rejected webhook with bad signature: {Message}", ex.Message);
            return BadRequest();
        }

        // Duplicate delivery — already handled, nothing to do.
        if (!await _repository.TryRecordEventAsync(stripeEvent.Id, stripeEvent.Type))
        {
            _logger.LogInformation("Ignoring duplicate event {EventId}", stripeEvent.Id);
            return Ok();
        }

        if (stripeEvent.Data.Object is not PaymentIntent intent)
            return Ok();

        var order = await _repository.GetByPaymentIntentAsync(intent.Id);
        if (order is null)
        {
            _logger.LogWarning("No order for payment intent {IntentId}", intent.Id);
            return Ok();
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await _repository.MarkPaidAsync(order.Id);
                _logger.LogInformation("Order {OrderNumber} paid", order.OrderNumber);
                break;

            case "payment_intent.payment_failed":
            case "payment_intent.canceled":
                await _repository.MarkFailedAndRestoreStockAsync(order.Id);
                _logger.LogInformation("Order {OrderNumber} failed, stock restored", order.OrderNumber);
                break;
        }
        return Ok();
    }
}