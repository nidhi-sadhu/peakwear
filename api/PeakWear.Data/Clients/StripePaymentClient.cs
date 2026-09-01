using Microsoft.Extensions.Configuration;
using PeakWear.Core.Services;
using Stripe;

namespace PeakWear.Data.Clients;

public class StripePaymentClient : IPaymentClient
{
    private readonly PaymentIntentService _intents;

    public StripePaymentClient(IConfiguration config)
    {
        var key = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        _intents = new PaymentIntentService(new StripeClient(key));
    }

    public async Task<PaymentIntentResult> CreateIntentAsync(
        long amountMinorUnits,
        string currency,
        string orderNumber,
        string? customerEmail,
        CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = amountMinorUnits,
            Currency = currency,
            ReceiptEmail = customerEmail,
            AutomaticPaymentMethods = new() { Enabled = true },

            // Metadata is arbitrary key/value stored on the intent. Not used for
            // lookup — it's there so a human debugging in the Stripe dashboard
            // can see which order a payment belongs to.
            Metadata = new Dictionary<string, string> { ["order_number"] = orderNumber }
        };

        // If the same order number is submitted twice (double-clicked button,
        // retried request), Stripe returns the original intent instead of
        // creating a second one and charging twice.
        var requestOptions = new RequestOptions { IdempotencyKey = $"order-{orderNumber}" };

        var intent = await _intents.CreateAsync(options, requestOptions, ct);
        return new PaymentIntentResult(intent.Id, intent.ClientSecret);
    }

    public async Task CancelIntentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        try
        {
            await _intents.CancelAsync(paymentIntentId, cancellationToken: ct);
        }
        catch (StripeException)
        {
            // Already succeeded, already cancelled, or gone. Nothing to undo,
            // and this runs from a background sweep — throwing helps no one.
        }
    }
}