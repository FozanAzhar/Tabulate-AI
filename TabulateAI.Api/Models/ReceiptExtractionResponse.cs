namespace TabulateAI.Api.Models;

public sealed class ReceiptExtractionResponse
{
    public string RawText { get; set; } = string.Empty;

    public string Merchant { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public string Category { get; set; } = "Other";

    public string CustomCategory { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public IReadOnlyList<ReceiptLineItem> LineItems { get; set; } = [];

    public bool IsReceipt { get; set; } = true;

    public double Confidence { get; set; }

    public IReadOnlyList<string> ValidationIssues { get; set; } = [];

    public string Source { get; set; } = "MistralOcr+Gemini";
}
