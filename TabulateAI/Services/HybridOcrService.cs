using Microsoft.Extensions.Logging;
using TabulateAI.Models;

namespace TabulateAI.Services;

public sealed class HybridOcrService : IOcrService
{
    private readonly AiExtractionOptions _options;
    private readonly CloudOcrService _cloudOcrService;
    private readonly ILocalOcrService _localOcrService;
    private readonly ILogger<HybridOcrService> _logger;

    public HybridOcrService(
        AiExtractionOptions options,
        CloudOcrService cloudOcrService,
        ILocalOcrService localOcrService,
        ILogger<HybridOcrService> logger)
    {
        _options = options;
        _cloudOcrService = cloudOcrService;
        _localOcrService = localOcrService;
        _logger = logger;
    }

    public async Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath)
    {
        if (_options.IsCloudEnabled)
        {
            try
            {
                _logger.LogInformation("Trying cloud AI extraction at {ApiBaseUrl}.", _options.ApiBaseUrl);
                var cloudResult = await _cloudOcrService.ExtractReceiptDataAsync(imagePath);

                if (HasUsableData(cloudResult))
                {
                    return cloudResult;
                }

                _logger.LogInformation("Cloud extraction returned little data; falling back to local OCR.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloud extraction failed; falling back to local OCR.");
            }
        }

        try
        {
            var localResult = await _localOcrService.ExtractReceiptDataAsync(imagePath);
            localResult.Source = "Local";
            return localResult;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local OCR failed.");
            return new OcrExtractionResult { Source = "Local" };
        }
    }

    private static bool HasUsableData(OcrExtractionResult result) =>
        !string.IsNullOrWhiteSpace(result.RawText) ||
        !string.IsNullOrWhiteSpace(result.Merchant) ||
        result.Amount.HasValue;
}
