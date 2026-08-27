using System.Text.Json;
using PeakWear.Core.Models.Recommendation;

namespace PeakWear.Core.Services;

public class SizeRecommendationService
{
    private readonly IProductRepository _productRepository;
    private readonly ISizeRecommendationClient _client;

    public SizeRecommendationService(
        IProductRepository productRepository,
        ISizeRecommendationClient client)
    {
        _productRepository = productRepository;
        _client = client;
    }

    public async Task<SizeRecommendationResponse?> RecommendAsync(
        SizeRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySlugAsync(request.ProductSlug);
        if (product is null) return null;

        var availableSizes = product.Variants
            .Select(v => v.Size)
            .Distinct()
            .ToList();

        if (availableSizes.Count == 0) return null;

        var prompt = BuildPrompt(product.Name, product.Category, availableSizes, request);

        try
        {
            var json = await _client.CompleteAsync(prompt, cancellationToken);
            var parsed = JsonSerializer.Deserialize<SizeRecommendationResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed is null || !availableSizes.Contains(parsed.RecommendedSize))
                return Fallback(availableSizes, request);

            return parsed;
        }
        catch
        {
            return Fallback(availableSizes, request);
        }
    }
    private static string BuildPrompt(
        string productName, string category, List<string> sizes,
        SizeRecommendationRequest request)
    {
        var bmi = request.WeightKg / Math.Pow(request.HeightCm / 100.0, 2);
        var sizeList = string.Join(", ", sizes);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You are a sizing assistant for an athletic wear retailer.");
        sb.AppendLine();
        sb.AppendLine($"Product: {productName} ({category})");
        sb.AppendLine($"Available sizes: {sizeList}");
        sb.AppendLine();
        sb.AppendLine("Customer:");
        sb.AppendLine($"- Height: {request.HeightCm} cm");
        sb.AppendLine($"- Weight: {request.WeightKg} kg");
        sb.AppendLine($"- BMI: {bmi:F1}");
        sb.AppendLine($"- Build: {request.Build}");
        sb.AppendLine($"- Preferred fit: {request.FitPreference}");
        sb.AppendLine();
        sb.AppendLine("Recommend ONE size from the available sizes list only.");
        sb.AppendLine("Athletic wear is typically close-fitting, and the customer's");
        sb.AppendLine("preferred fit should influence the choice.");
        sb.AppendLine();
        sb.AppendLine("Respond with JSON only, using these exact property names:");
        sb.AppendLine("recommendedSize (string), reasoning (string), alternative (string or null).");
        sb.AppendLine($"recommendedSize must be one of: {sizeList}");
        sb.AppendLine("reasoning should be two sentences addressed to the customer, with no medical advice.");

        return sb.ToString();
    }

    // Deterministic fallback so the feature degrades rather than failing
    private static SizeRecommendationResponse Fallback(
        List<string> sizes, SizeRecommendationRequest request)
    {
        var bmi = request.WeightKg / Math.Pow(request.HeightCm / 100.0, 2);
        var index = bmi switch
        {
            < 20 => 0,
            < 26 => Math.Min(1, sizes.Count - 1),
            _    => sizes.Count - 1
        };

        return new SizeRecommendationResponse
        {
            RecommendedSize = sizes[index],
            Reasoning = "Based on your height and weight, this size should fit well. " +
                        "Sizing guidance is approximate — check the size chart if you're between sizes.",
            Alternative = null
        };
    }
}