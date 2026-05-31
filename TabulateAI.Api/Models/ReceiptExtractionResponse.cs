namespace TabulateAI.Api.Models;

public sealed class ReceiptExtractionResponse
{
    public string RawText { get; set; } = string.Empty;

    public string Merchant { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public string Category { get; set; } = "Other";

    public bool IsReceipt { get; set; } = true;

    public double Confidence { get; set; }

    public IReadOnlyList<string> ValidationIssues { get; set; } = [];

    public string Source { get; set; } = "MistralOcr+Gemini";
}
