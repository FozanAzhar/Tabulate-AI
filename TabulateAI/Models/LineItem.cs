namespace TabulateAI.Models;

public class LineItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsDiscount { get; set; }

    public string PriceFormatted => IsDiscount
        ? $"-{Math.Abs(Price):C2}"
        : Price.ToString("C2");
}
