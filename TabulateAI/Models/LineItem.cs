namespace TabulateAI.Models;

public class LineItem
{
    public string Name { get; set; } = string.Empty;

    public string Quantity { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsDiscount { get; set; }

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Quantity)
            ? Name
            : $"{Quantity.Trim()} {Name}".Trim();

    public string PriceFormatted => IsDiscount
        ? $"-{Math.Abs(Price):C2}"
        : Price.ToString("C2");

    public bool ShowDivider { get; set; }
}
