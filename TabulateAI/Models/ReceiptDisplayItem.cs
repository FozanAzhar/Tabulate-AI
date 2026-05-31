namespace TabulateAI.Models;

public class ReceiptDisplayItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Icon { get; set; } = "🧾";
    public Color IconBackground { get; set; } = Colors.White;
    public Color IconColor { get; set; } = Colors.Black;
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;

    public string AmountFormatted => Amount.ToString("C2");
}
