namespace TabulateAI.Models;

public class CategoryBudgetStatus
{
    private const double MaxBarWidth = 280;

    public string Category { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    public decimal Spent { get; set; }

    public double Progress => Budget > 0 ? Math.Min(1.0, (double)(Spent / Budget)) : 0;

    public double BarFillWidth => Math.Round(Progress * MaxBarWidth, 1);

    public string StatusLabel => $"{Spent:C0} of {Budget:C0}";

    public bool IsOverBudget => Budget > 0 && Spent > Budget;

    public string OverBudgetLabel =>
        IsOverBudget ? $"Over by {(Spent - Budget):C0}" : string.Empty;

    public Color BarColor => IsOverBudget
        ? Color.FromArgb("#EF4444")
        : Color.FromArgb("#7C3AED");
}
