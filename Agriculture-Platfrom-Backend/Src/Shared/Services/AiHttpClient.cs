using System.Text;
using System.Text.Json;
using AgriculturalMonitorSystem.Config;
using AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;

namespace AgriculturalMonitorSystem.Src.Shared.Services;

public class AiHttpClient
{
    private readonly HttpClient _http;
    private readonly AiSettings _settings;
    private readonly ILogger<AiHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null  // preserve exact property names (PascalCase / snake_case via attributes)
    };

    public AiHttpClient(HttpClient http, AiSettings settings, ILogger<AiHttpClient> logger)
    {
        _http     = http;
        _settings = settings;
        _logger   = logger;
    }

    public async Task<string> AskAsync(AiRequestDto request)
    {
        var json    = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending question to AI service: {BaseUrl}", _settings.BaseUrl);

        var response = await _http.PostAsync(_settings.BaseUrl, content);
        var body     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI service returned {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException($"AI service error ({response.StatusCode}): {body}");
        }

        // AI service returns { "answer": "..." }
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.TryGetProperty("answer", out var answerProp)
            ? answerProp.GetString() ?? body
            : body;
    }
}
