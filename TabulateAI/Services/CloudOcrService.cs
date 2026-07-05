using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TabulateAI.Models;

namespace TabulateAI.Services;

public sealed class CloudOcrService : IOcrService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiExtractionOptions _options;
    private readonly ILogger<CloudOcrService> _logger;

    public CloudOcrService(HttpClient httpClient, AiExtractionOptions options, ILogger<CloudOcrService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        if (_options.IsCloudEnabled && _options.ApiBaseUrl is not null)
        {
            _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(180);
        }
    }

    public async Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath)
    {
        if (!_options.IsCloudEnabled || _options.ApiBaseUrl is null)
        {
            throw new InvalidOperationException("Cloud extraction is not configured.");
        }

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return new OcrExtractionResult();
        }

        await using var fileStream = File.OpenRead(imagePath);
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(imagePath));
        content.Add(streamContent, "file", Path.GetFileName(imagePath));

        using var response = await _httpClient.PostAsync("/api/receipts/extract", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Cloud extraction failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Cloud AI extraction failed ({(int)response.StatusCode}).");
        }

        var cloudResult = JsonSerializer.Deserialize<CloudExtractionDto>(body, JsonOptions);
        if (cloudResult is null)
        {
            throw new InvalidOperationException("Cloud AI returned an empty response.");
        }

        return new OcrExtractionResult
        {
            RawText = cloudResult.RawText ?? string.Empty,
            Merchant = cloudResult.Merchant ?? string.Empty,
            Amount = cloudResult.Amount,
            Date = cloudResult.Date,
            SuggestedCategory = string.IsNullOrWhiteSpace(cloudResult.Category)
                ? ExpenseCategories.Other
                : cloudResult.Category,
            CustomCategory = cloudResult.CustomCategory ?? string.Empty,
            Location = cloudResult.Location ?? string.Empty,
            PaymentMethod = cloudResult.PaymentMethod ?? string.Empty,
            LineItems = (cloudResult.LineItems ?? [])
                .Select(item => new LineItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity ?? string.Empty,
                    Price = item.Price,
                    IsDiscount = item.IsDiscount
                })
                .ToList(),
            Source = cloudResult.Source ?? "MistralOcr+Gemini",
            Confidence = cloudResult.Confidence,
            IsReceipt = cloudResult.IsReceipt,
            ValidationIssues = cloudResult.ValidationIssues ?? []
        };
    }

    private static string GetContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "image/jpeg"
        };

    private sealed class CloudExtractionDto
    {
        public string? RawText { get; set; }

        public string? Merchant { get; set; }

        public decimal? Amount { get; set; }

        public DateTime? Date { get; set; }

        public string? Category { get; set; }

        public string? CustomCategory { get; set; }

        public string? Location { get; set; }

        public string? PaymentMethod { get; set; }

        public List<CloudLineItemDto>? LineItems { get; set; }

        public bool IsReceipt { get; set; } = true;

        public double Confidence { get; set; }

        public List<string>? ValidationIssues { get; set; }

        public string? Source { get; set; }
    }

    private sealed class CloudLineItemDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Quantity { get; set; }

        public decimal Price { get; set; }

        public bool IsDiscount { get; set; }
    }
}
