using PeakWear.Core.Services;

namespace PeakWear.Api.Services;

// Reserved stock has to come back when a checkout is abandoned, otherwise
// every closed tab permanently removes an item from the catalogue.
public class PendingOrderSweeper : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PendingOrderSweeper> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(30);

    public PendingOrderSweeper(IServiceProvider services, ILogger<PendingOrderSweeper> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(ct);
            }
            catch (Exception ex)
            {
                // Never let one bad pass kill the loop — it won't restart.
                _logger.LogError(ex, "Sweep failed");
            }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        // This service is a singleton but the repository is scoped, so it needs
        // its own scope per pass rather than a constructor injection.
        using var scope = _services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var payments = scope.ServiceProvider.GetRequiredService<IPaymentClient>();

        var stale = await repository.GetStalePendingOrdersAsync(DateTime.UtcNow - StaleAfter);
        if (stale.Count == 0) return;

        foreach (var order in stale)
        {
            // Cancel at Stripe first. If we released stock and the customer then
            // completed a half-finished payment, we'd owe them an item we no
            // longer have reserved.
            if (!string.IsNullOrEmpty(order.StripePaymentIntentId))
                await payments.CancelIntentAsync(order.StripePaymentIntentId, ct);

            await repository.ExpireAndRestoreStockAsync(order.Id);
            _logger.LogInformation("Expired {OrderNumber}, stock restored", order.OrderNumber);
        }
    }
}