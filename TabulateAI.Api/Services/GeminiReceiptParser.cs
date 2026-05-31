using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TabulateAI.Api.Models;
using TabulateAI.Api.Options;

namespace TabulateAI.Api.Services;

public sealed class GeminiReceiptParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<GeminiReceiptParser> _logger;

    public GeminiReceiptParser(HttpClient httpClient, IOptions<AiOptions> options, ILogger<GeminiReceiptParser> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ReceiptExtractionResponse> ParseAsync(string ocrText, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var prompt = $"""
            You are validating and parsing receipt OCR text for an expense tracker app.
            Extract structured fields and validate the content.

            Rules:
            - merchant: store or business name
            - amount: total paid (numeric only, no currency symbol)
            - date: ISO yyyy-MM-dd if possible
            - category: one of Food, Transport, Shopping, Bills, Other
            - isReceipt: false if the text does not look like a purchase receipt
            - confidence: 0.0 to 1.0 for overall extraction quality
            - issues: short validation warnings (empty array if none)

            OCR text:
            {ocrText}
            """;

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1,
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        merchant = new { type = "STRING" },
                        amount = new { type = "NUMBER" },
                        date = new { type = "STRING" },
                        category = new
                        {
                            type = "STRING",
                            @enum = new[] { "Food", "Transport", "Shopping", "Bills", "Other" }
                        },
                        isReceipt = new { type = "BOOLEAN" },
                        confidence = new { type = "NUMBER" },
                        issues = new
                        {
                            type = "ARRAY",
                            items = new { type = "STRING" }
                        }
                    },
                    required = new[] { "merchant", "amount", "date", "category", "isReceipt", "confidence", "issues" }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.GeminiModel}:generateContent?key={Uri.EscapeDataString(_options.GeminiApiKey)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini parsing failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gemini request failed ({(int)response.StatusCode}).");
        }

        return MapGeminiResponse(body, ocrText);
    }

    private static ReceiptExtractionResponse MapGeminiResponse(string geminiJson, string ocrText)
    {
        using var document = JsonDocument.Parse(geminiJson);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        using var parsed = JsonDocument.Parse(text);
        var root = parsed.RootElement;

        DateTime? date = null;
        if (root.TryGetProperty("date", out var dateElement) &&
            DateTime.TryParse(dateElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            date = parsedDate.Date;
        }

        decimal? amount = null;
        if (root.TryGetProperty("amount", out var amountElement) && amountElement.ValueKind == JsonValueKind.Number)
        {
            amount = amountElement.GetDecimal();
        }

        var issues = new List<string>();
        if (root.TryGetProperty("issues", out var issuesElement) && issuesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var issue in issuesElement.EnumerateArray())
            {
                if (issue.ValueKind == JsonValueKind.String)
                {
                    var value = issue.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        issues.Add(value);
                    }
                }
            }
        }

        return new ReceiptExtractionResponse
        {
            RawText = ocrText,
            Merchant = root.TryGetProperty("merchant", out var merchant) ? merchant.GetString() ?? string.Empty : string.Empty,
            Amount = amount,
            Date = date,
            Category = root.TryGetProperty("category", out var category) ? category.GetString() ?? "Other" : "Other",
            IsReceipt = root.TryGetProperty("isReceipt", out var isReceipt) && isReceipt.GetBoolean(),
            Confidence = root.TryGetProperty("confidence", out var confidence) && confidence.ValueKind == JsonValueKind.Number
                ? confidence.GetDouble()
                : 0.5,
            ValidationIssues = issues,
            Source = "MistralOcr+Gemini"
        };
    }
}
