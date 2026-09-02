using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.DbModels;

// Stripe retries webhooks and can deliver the same event more than once,
// sometimes out of order. The primary key is Stripe's own event id, so a
// duplicate insert fails and we know we've already handled it.
public class ProcessedStripeEvent
{
    [Key, StringLength(64)]
    public string Id { get; set; } = "";      // evt_...

    [Required, StringLength(64)]
    public string Type { get; set; } = "";    // payment_intent.succeeded

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}