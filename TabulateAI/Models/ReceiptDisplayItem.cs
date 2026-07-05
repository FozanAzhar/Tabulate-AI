namespace TabulateAI.Models;

using TabulateAI.Helpers;

public class ReceiptDisplayItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string IconSource { get; set; } = AppIcons.Receipt;
    public Color IconBackground { get; set; } = Colors.White;
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;

    public bool HasMerchantLogo { get; set; }

    public string AmountFormatted => Amount.ToString("C2");

    public bool ShowDivider { get; set; }

    public bool IsFirstInGroup { get; set; }

    public bool IsLastInGroup { get; set; }

    public CornerRadius ItemCornerRadius { get; set; }
}
