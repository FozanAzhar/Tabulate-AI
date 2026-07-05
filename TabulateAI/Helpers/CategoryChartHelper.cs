using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class CategoryChartHelper
{
    public const double StackedBarTotalWidth = 300;
    public const double CategoryBarMaxWidth = 300;

    public static Color GetCategoryColor(string category)
    {
        if (category.Equals(ExpenseCategories.Food, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#7C3AED");
        }

        if (category.Equals(ExpenseCategories.Groceries, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#8B5CF6");
        }

        if (category.Equals(ExpenseCategories.Transport, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#F59E0B");
        }

        if (category.Equals(ExpenseCategories.Shopping, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#3B82F6");
        }

        if (category.Equals(ExpenseCategories.Home, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#6366F1");
        }

        if (category.Equals(ExpenseCategories.Bills, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#10B981");
        }

        if (category.Equals(ExpenseCategories.Health, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#EC4899");
        }

        if (category.Equals(ExpenseCategories.Entertainment, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#F97316");
        }

        if (category.Equals(ExpenseCategories.Travel, StringComparison.OrdinalIgnoreCase))
        {
            return Color.FromArgb("#0EA5E9");
        }

        return Color.FromArgb("#A1A1AA");
    }

    public static List<CategoryBreakdownItem> BuildBreakdown(IReadOnlyList<CategorySummary> summaries, decimal total)
    {
        if (summaries.Count == 0 || total <= 0)
        {
            return [];
        }

        return summaries
            .OrderByDescending(s => s.Total)
            .Select(s => new CategoryBreakdownItem
            {
                Category = s.Category,
                Amount = s.Total,
                Percent = (double)(s.Total / total),
                BarWidth = (double)(s.Total / total),
                BarColor = GetCategoryColor(s.Category),
                LegendColor = GetCategoryColor(s.Category)
            })
            .ToList();
    }

    public static List<StackedBarSegment> BuildStackedBar(IReadOnlyList<CategorySummary> summaries, decimal total)
    {
        if (summaries.Count == 0 || total <= 0)
        {
            return [];
        }

        return summaries
            .Where(s => s.Total > 0)
            .OrderByDescending(s => s.Total)
            .Select(summary => new StackedBarSegment
            {
                Color = GetCategoryColor(summary.Category),
                Share = (double)(summary.Total / total)
            })
            .ToList();
    }

    public static List<ReportLegendItem> BuildLegend(IReadOnlyList<CategorySummary> summaries)
    {
        return summaries
            .OrderByDescending(s => s.Total)
            .Take(4)
            .Select(s => new ReportLegendItem
            {
                Label = s.Category,
                Color = GetCategoryColor(s.Category)
            })
            .ToList();
    }
}
