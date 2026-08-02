using TabulateAI.Api.Models;
using TabulateAI.Api.Options;
using Microsoft.Extensions.Options;

namespace TabulateAI.Api.Services;

public sealed class ReceiptAiPipeline
{
    private readonly MistralOcrClient _mistralOcrClient;
    private readonly GeminiReceiptParser _geminiReceiptParser;
    private readonly AiOptions _options;
    private readonly ILogger<ReceiptAiPipeline> _logger;

    public ReceiptAiPipeline(
        MistralOcrClient mistralOcrClient,
        GeminiReceiptParser geminiReceiptParser,
        IOptions<AiOptions> options,
        ILogger<ReceiptAiPipeline> logger)
    {
        _mistralOcrClient = mistralOcrClient;
        _geminiReceiptParser = geminiReceiptParser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ReceiptExtractionResponse> ExtractAsync(
        byte[] imageBytes,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Gemini reads the receipt image directly.
        try
        {
            _logger.LogInformation("Starting Gemini image extraction.");
            var geminiResult = await _geminiReceiptParser.ParseImageAsync(imageBytes, contentType, cancellationToken);
            if (HasUsefulExtraction(geminiResult))
            {
                return geminiResult;
            }

            _logger.LogWarning("Gemini returned little useful data; trying Mistral OCR fallback.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini image extraction failed; trying Mistral OCR fallback.");
        }

        if (!_options.HasMistral)
        {
            return new ReceiptExtractionResponse
            {
                Source = "Gemini",
                ValidationIssues = ["AI extraction failed and no Mistral OCR fallback key is configured."],
                Confidence = 0,
                IsReceipt = false
            };
        }

        _logger.LogInformation("Starting Mistral OCR fallback.");
        string ocrText;
        try
        {
            ocrText = await _mistralOcrClient.ExtractTextAsync(imageBytes, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mistral OCR fallback failed.");
            return new ReceiptExtractionResponse
            {
                Source = "Gemini+MistralFallback",
                ValidationIssues = ["Gemini and Mistral extraction both failed."],
                Confidence = 0,
                IsReceipt = false
            };
        }

        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return new ReceiptExtractionResponse
            {
                Source = "MistralOcr+Gemini",
                ValidationIssues = ["No text could be extracted from the image."],
                Confidence = 0,
                IsReceipt = false
            };
        }

        _logger.LogInformation("Parsing Mistral OCR text with Gemini.");
        return await _geminiReceiptParser.ParseAsync(ocrText, source: "MistralOcr+Gemini", cancellationToken);
    }

    private static bool HasUsefulExtraction(ReceiptExtractionResponse result) =>
        !string.IsNullOrWhiteSpace(result.Merchant)
        || result.Amount is > 0
        || result.LineItems.Count > 0
        || !string.IsNullOrWhiteSpace(result.RawText);
}
