using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PeakWear.Core.Services;

namespace PeakWear.Data.Clients;

public class GroqClient :  ISizeRecommendationClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string Model = "openai/gpt-oss-20b";
    private const string Url = "https://api.groq.com/openai/v1/chat/completions";

    public GroqClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Groq:ApiKey"]
                  ?? throw new InvalidOperationException("Groq:ApiKey is not configured.");
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2,                              
            max_tokens = 400,
            response_format = new { type = "json_object" } 
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"GROQ ERROR: {raw}");
            response.EnsureSuccessStatusCode();
        }

        using var doc = JsonDocument.Parse(raw);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}