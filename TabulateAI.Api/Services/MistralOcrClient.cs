using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TabulateAI.Api.Options;

namespace TabulateAI.Api.Services;

public sealed class MistralOcrClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<MistralOcrClient> _logger;

    public MistralOcrClient(HttpClient httpClient, IOptions<AiOptions> options, ILogger<MistralOcrClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(byte[] imageBytes, string contentType, CancellationToken cancellationToken = default)
    {
        if (!_options.HasMistral)
        {
            throw new InvalidOperationException("Mistral API key is not configured.");
        }

        var base64 = Convert.ToBase64String(imageBytes);
        var mime = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;
        var dataUri = $"data:{mime};base64,{base64}";

        var payload = new
        {
            model = _options.MistralOcrModel,
            document = new
            {
                type = "image_url",
                image_url = dataUri
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mistral.ai/v1/ocr");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.MistralApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Mistral OCR failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Mistral OCR request failed ({(int)response.StatusCode}).");
        }

        return ExtractTextFromResponse(body);
    }

    private static string ExtractTextFromResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
        {
            return textElement.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("pages", out var pages) && pages.ValueKind == JsonValueKind.Array)
        {
            var builder = new StringBuilder();
            foreach (var page in pages.EnumerateArray())
            {
                if (page.TryGetProperty("markdown", out var markdown) && markdown.ValueKind == JsonValueKind.String)
                {
                    builder.AppendLine(markdown.GetString());
                }
                else if (page.TryGetProperty("text", out var pageText) && pageText.ValueKind == JsonValueKind.String)
                {
                    builder.AppendLine(pageText.GetString());
                }
            }

            return builder.ToString().Trim();
        }

        return string.Empty;
    }
}
