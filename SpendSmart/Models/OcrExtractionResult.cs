namespace SpendSmart.Models;

public class OcrExtractionResult
{
    public string RawText { get; set; } = string.Empty;

    public string Merchant { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public string SuggestedCategory { get; set; } = ExpenseCategories.Other;
}
