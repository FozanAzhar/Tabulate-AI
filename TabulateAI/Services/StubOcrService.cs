using TabulateAI.Models;

namespace TabulateAI.Services;

public class StubOcrService : IOcrService, ILocalOcrService
{
    public Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath) =>
        Task.FromResult(new OcrExtractionResult
        {
            RawText = string.Empty,
            Merchant = string.Empty,
            SuggestedCategory = ExpenseCategories.Other
        });
}
