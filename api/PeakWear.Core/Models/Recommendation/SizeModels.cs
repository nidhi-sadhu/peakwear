using System.ComponentModel.DataAnnotations;

namespace PeakWear.Core.Models.Recommendation;

public class SizeRecommendationRequest
{
    [Required]
    public string ProductSlug { get; set; } = "";

    [Range(120, 220)]
    public int HeightCm { get; set; }

    [Range(35, 200)]
    public int WeightKg { get; set; }

    // Slim, Average, Athletic, Curvy
    [Required, StringLength(16)]
    public string Build { get; set; } = "Average";

    // Snug, Regular, Relaxed
    [Required, StringLength(16)]
    public string FitPreference { get; set; } = "Regular";
}

public class SizeRecommendationResponse
{
    public string RecommendedSize { get; set; } = "";
    public string Reasoning { get; set; } = "";
    public string? Alternative { get; set; }
}