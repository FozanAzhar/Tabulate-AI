using SpendSmart.Models;

namespace SpendSmart.Services;

public interface IOcrService
{
    Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath);
}
