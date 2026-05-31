using TabulateAI.Api.Models;

namespace TabulateAI.Api.Services;

public sealed class ReceiptAiPipeline
{
    private readonly MistralOcrClient _mistralOcrClient;
    private readonly GeminiReceiptParser _geminiReceiptParser;
    private readonly ILogger<ReceiptAiPipeline> _logger;

    public ReceiptAiPipeline(
        MistralOcrClient mistralOcrClient,
        GeminiReceiptParser geminiReceiptParser,
        ILogger<ReceiptAiPipeline> logger)
    {
        _mistralOcrClient = mistralOcrClient;
        _geminiReceiptParser = geminiReceiptParser;
        _logger = logger;
    }

    public async Task<ReceiptExtractionResponse> ExtractAsync(
        byte[] imageBytes,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Mistral OCR extraction.");
        var ocrText = await _mistralOcrClient.ExtractTextAsync(imageBytes, contentType, cancellationToken);

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

        _logger.LogInformation("Starting Gemini validation and parsing.");
        return await _geminiReceiptParser.ParseAsync(ocrText, cancellationToken);
    }
}
