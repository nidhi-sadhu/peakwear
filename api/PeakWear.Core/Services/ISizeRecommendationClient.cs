namespace PeakWear.Core.Services;

public interface ISizeRecommendationClient
{
    Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default);
}