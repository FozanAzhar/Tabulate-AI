using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TabulateAI.Api.Helpers;
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

    public Task<ReceiptExtractionResponse> ParseAsync(string ocrText, CancellationToken cancellationToken = default) =>
        ParseAsync(ocrText, source: "MistralOcr+Gemini", cancellationToken);

    public async Task<ReceiptExtractionResponse> ParseImageAsync(
        byte[] imageBytes,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!_options.HasGemini)
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var mime = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType.Split(';', 2)[0].Trim();
        var base64 = Convert.ToBase64String(imageBytes);
        var prompt = BuildPrompt(
            "Read the receipt image directly and extract structured fields.",
            imageContext: true);

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mime,
                                data = base64
                            }
                        }
                    }
                }
            },
            generationConfig = BuildGenerationConfig()
        };

        var body = await SendGeminiAsync(payload, cancellationToken);
        return MapGeminiResponse(body, ocrText: string.Empty, source: "Gemini");
    }

    public async Task<ReceiptExtractionResponse> ParseAsync(
        string ocrText,
        string source,
        CancellationToken cancellationToken = default)
    {
        if (!_options.HasGemini)
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var prompt = BuildPrompt(
            $"""
            You are validating and parsing receipt OCR text for an expense tracker app.
            Extract structured fields and validate the content.

            OCR text:
            {ocrText}
            """,
            imageContext: false);

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
            generationConfig = BuildGenerationConfig()
        };

        var body = await SendGeminiAsync(payload, cancellationToken);
        return MapGeminiResponse(body, ocrText, source);
    }

    private async Task<string> SendGeminiAsync(object payload, CancellationToken cancellationToken)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_options.GeminiModel}:generateContent?key={Uri.EscapeDataString(_options.GeminiApiKey)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini request failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gemini request failed ({(int)response.StatusCode}).");
        }

        return body;
    }

    private static string BuildPrompt(string intro, bool imageContext)
    {
        var sourceLine = imageContext
            ? "Read the attached receipt image carefully."
            : "Use the OCR text provided below.";

        return $"""
            {intro}
            {sourceLine}

            Rules:
            - merchant: store or business name (not the street address)
            - amount: total paid (numeric only, no currency symbol)
            - date: purchase/transaction date as ISO yyyy-MM-dd only.
              Australian receipts usually use dd/MM/yyyy — interpret day-first unless clearly US format.
              Use the sale/transaction date, NOT today's date, print timestamps, or copyright years.
              If unsure, leave empty and add an issue.
            - category: best matching preset — one of Food, Groceries, Transport, Shopping, Home, Bills, Health, Entertainment, Travel, Other
            - customCategory: optional more specific label ONLY when it adds meaning beyond the preset
              (e.g. "Fuel & Gas" for a petrol station, "Pharmacy" for a chemist). Leave empty if the preset is enough.
            - location: full merchant address printed on the receipt (street, city, etc.). Empty if not visible.
            - paymentMethod: card type or payment method if shown (e.g. MASTERCARD, VISA, CASH). Empty if unknown.
            - lineItems: EVERY product/service row printed on the receipt — critical for grocery stores
              (Woolworths, Coles, Aldi, etc.), fuel pumps, restaurants, and retail.
              For each row include:
              - name: product description exactly as shown (e.g. "Bananas", "FULL CREAM MILK 2L")
              - quantity: optional qty/volume prefix when shown (e.g. "2", "3x", "1.5kg", "10.5L") — empty if not shown
              - price: the line total for that item (numeric only)
              - isDiscount: true for discounts, savings, or negative amounts
              Do NOT include subtotal, tax, GST, or grand-total summary rows — only purchased items.
              If the receipt lists 15 products, return all 15 in lineItems.
            - isReceipt: false if the text does not look like a purchase receipt
            - confidence: 0.0 to 1.0 for overall extraction quality
            - issues: short validation warnings (empty array if none)
            - rawText: best-effort plain text transcription of the receipt (can be empty)
            """;
    }

    private static object BuildGenerationConfig() => new
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
                    @enum = new[]
                    {
                        "Food", "Groceries", "Transport", "Shopping", "Home", "Bills", "Health",
                        "Entertainment", "Travel", "Other"
                    }
                },
                customCategory = new { type = "STRING" },
                location = new { type = "STRING" },
                paymentMethod = new { type = "STRING" },
                rawText = new { type = "STRING" },
                lineItems = new
                {
                    type = "ARRAY",
                    items = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            name = new { type = "STRING" },
                            quantity = new { type = "STRING" },
                            price = new { type = "NUMBER" },
                            isDiscount = new { type = "BOOLEAN" }
                        },
                        required = new[] { "name", "price" }
                    }
                },
                isReceipt = new { type = "BOOLEAN" },
                confidence = new { type = "NUMBER" },
                issues = new
                {
                    type = "ARRAY",
                    items = new { type = "STRING" }
                }
            },
            required = new[]
            {
                "merchant", "amount", "date", "category", "isReceipt", "confidence", "issues", "lineItems"
            }
        }
    };

    private static ReceiptExtractionResponse MapGeminiResponse(string geminiJson, string ocrText, string source)
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

        var rawText = ocrText;
        if (root.TryGetProperty("rawText", out var rawTextElement) &&
            rawTextElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(rawTextElement.GetString()))
        {
            rawText = rawTextElement.GetString() ?? ocrText;
        }

        DateTime? date = null;
        if (root.TryGetProperty("date", out var dateElement))
        {
            date = ReceiptDateHelper.Parse(dateElement.GetString())
                ?? ReceiptDateHelper.ParseFromOcrText(rawText);
        }
        else
        {
            date = ReceiptDateHelper.ParseFromOcrText(rawText);
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

        if (!date.HasValue && root.TryGetProperty("date", out var rawDateElement) &&
            !string.IsNullOrWhiteSpace(rawDateElement.GetString()))
        {
            issues.Add("Receipt date could not be parsed — please verify manually.");
        }
        else if (!date.HasValue)
        {
            issues.Add("No receipt date found — please enter the date manually.");
        }

        var lineItems = new List<ReceiptLineItem>();
        if (root.TryGetProperty("lineItems", out var lineItemsElement) && lineItemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in lineItemsElement.EnumerateArray())
            {
                if (!item.TryGetProperty("name", out var nameElement))
                {
                    continue;
                }

                var name = nameElement.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!item.TryGetProperty("price", out var priceElement) || priceElement.ValueKind != JsonValueKind.Number)
                {
                    continue;
                }

                var isDiscount = item.TryGetProperty("isDiscount", out var discountElement)
                    && discountElement.ValueKind == JsonValueKind.True;

                lineItems.Add(new ReceiptLineItem
                {
                    Name = name,
                    Quantity = item.TryGetProperty("quantity", out var quantityElement)
                        ? quantityElement.GetString()?.Trim() ?? string.Empty
                        : string.Empty,
                    Price = priceElement.GetDecimal(),
                    IsDiscount = isDiscount
                });
            }
        }

        return new ReceiptExtractionResponse
        {
            RawText = rawText,
            Merchant = root.TryGetProperty("merchant", out var merchant) ? merchant.GetString() ?? string.Empty : string.Empty,
            Amount = amount,
            Date = date,
            Category = root.TryGetProperty("category", out var category) ? category.GetString() ?? "Other" : "Other",
            CustomCategory = root.TryGetProperty("customCategory", out var customCategory)
                ? customCategory.GetString()?.Trim() ?? string.Empty
                : string.Empty,
            Location = root.TryGetProperty("location", out var location)
                ? location.GetString()?.Trim() ?? string.Empty
                : string.Empty,
            PaymentMethod = root.TryGetProperty("paymentMethod", out var paymentMethod)
                ? paymentMethod.GetString()?.Trim() ?? string.Empty
                : string.Empty,
            LineItems = lineItems,
            IsReceipt = root.TryGetProperty("isReceipt", out var isReceipt) && isReceipt.GetBoolean(),
            Confidence = root.TryGetProperty("confidence", out var confidence) && confidence.ValueKind == JsonValueKind.Number
                ? confidence.GetDouble()
                : 0.5,
            ValidationIssues = issues,
            Source = source
        };
    }
}
