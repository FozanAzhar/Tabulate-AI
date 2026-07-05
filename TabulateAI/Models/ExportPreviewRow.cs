namespace TabulateAI.Models;

public class ExportPreviewRow
{
    public string Date { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountDisplay { get; set; } = string.Empty;
    public bool IsHeader { get; set; }
    public bool IsSummary { get; set; }
    public bool IsDivider { get; set; }
}
