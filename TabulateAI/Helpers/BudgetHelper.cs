using System.Globalization;
using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class BudgetHelper
{
    private const string PreferencesKey = "category_budgets";

    public static IReadOnlyDictionary<string, decimal> GetAll()
    {
        var stored = Preferences.Default.Get(PreferencesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in stored.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf(':');
            if (separator <= 0 || separator >= part.Length - 1)
            {
                continue;
            }

            var category = part[..separator].Trim();
            var amountText = part[(separator + 1)..].Trim();
            if (decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && amount > 0)
            {
                result[category] = amount;
            }
        }

        return result;
    }

    public static void SetBudget(string category, decimal amount)
    {
        var trimmed = category.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        var budgets = GetAll().ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        if (amount <= 0)
        {
            budgets.Remove(trimmed);
        }
        else
        {
            budgets[trimmed] = amount;
        }

        SaveAll(budgets);
    }

    public static void RemoveBudget(string category)
    {
        var budgets = GetAll().ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        budgets.Remove(category);
        SaveAll(budgets);
    }

    public static List<CategoryBudgetStatus> BuildStatus(IEnumerable<CategorySummary> summaries)
    {
        var budgets = GetAll();
        if (budgets.Count == 0)
        {
            return [];
        }

        var spentByCategory = summaries.ToDictionary(
            s => s.Category,
            s => s.Total,
            StringComparer.OrdinalIgnoreCase);

        return budgets
            .Select(b => new CategoryBudgetStatus
            {
                Category = b.Key,
                Budget = b.Value,
                Spent = spentByCategory.TryGetValue(b.Key, out var spent) ? spent : 0
            })
            .OrderByDescending(b => b.Progress)
            .ThenBy(b => b.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void SaveAll(IReadOnlyDictionary<string, decimal> budgets)
    {
        var encoded = string.Join('|', budgets
            .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kvp => $"{kvp.Key}:{kvp.Value.ToString(CultureInfo.InvariantCulture)}"));

        Preferences.Default.Set(PreferencesKey, encoded);
    }
}
