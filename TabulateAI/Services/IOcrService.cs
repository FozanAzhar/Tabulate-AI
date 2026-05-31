using TabulateAI.Models;

namespace TabulateAI.Services;

public interface IOcrService
{
    Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath);
}
