namespace TabulateAI.Models;

public class CategorySummary
{
    public string Category { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public int Count { get; set; }

    public double ShareOfTotal { get; set; }

    public Color BarColor { get; set; } = Colors.Gray;
}
