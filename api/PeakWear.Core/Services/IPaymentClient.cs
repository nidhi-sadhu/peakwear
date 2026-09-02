namespace PeakWear.Core.Services;

public record PaymentIntentResult(string PaymentIntentId, string ClientSecret);

public interface IPaymentClient
{
    // Amount is in minor units (cents). Integers only — never float money.
    Task<PaymentIntentResult> CreateIntentAsync(
        long amountMinorUnits,
        string currency,
        string orderNumber,
        string? customerEmail,
        CancellationToken ct = default);

    Task CancelIntentAsync(string paymentIntentId, CancellationToken ct = default);
}