using TabulateAI.Models;

namespace TabulateAI.Services;

public interface ILocalOcrService
{
    Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath);
}
