using SpendSmart.Models;

namespace SpendSmart.Services;

public class StubOcrService : IOcrService
{
    public Task<OcrExtractionResult> ExtractReceiptDataAsync(string imagePath) =>
        Task.FromResult(new OcrExtractionResult
        {
            RawText = string.Empty,
            Merchant = string.Empty,
            SuggestedCategory = ExpenseCategories.Other
        });
}
