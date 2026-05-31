namespace TabulateAI.Models;

public class CategoryBreakdownItem
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percent { get; set; }
    public double BarWidth { get; set; }
    public Color BarColor { get; set; } = Colors.Gray;
    public Color LegendColor { get; set; } = Colors.Gray;

    public string AmountLabel => $"{Amount:C0} · {Percent:P0}";
}
