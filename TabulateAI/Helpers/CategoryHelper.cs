using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class CategoryHelper
{
    private const string PreferencesKey = "custom_expense_categories";

    public static IReadOnlyList<string> GetAllCategories()
    {
        var custom = GetCustomCategories();
        return ExpenseCategories.All
            .Concat(custom)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> GetCustomCategories()
    {
        var stored = Preferences.Default.Get(PreferencesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return [];
        }

        return stored
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static void AddCustomCategory(string category)
    {
        var trimmed = category.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        if (ExpenseCategories.All.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var custom = GetCustomCategories().ToList();
        if (custom.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        custom.Add(trimmed);
        Preferences.Default.Set(PreferencesKey, string.Join('|', custom));
    }

    public static bool IsPresetCategory(string category) =>
        ExpenseCategories.All.Contains(category, StringComparer.OrdinalIgnoreCase);
}
