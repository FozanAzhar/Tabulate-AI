using SpendSmart.Models;

namespace SpendSmart.Services;

public static class CategorySuggestionService
{
    private static readonly Dictionary<string, string[]> KeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [ExpenseCategories.Food] =
        [
            "cafe", "coffee", "restaurant", "grocery", "woolworths", "coles", "aldi", "mcdonald",
            "kfc", "subway", "pizza", "bakery", "food", "starbucks", "uber eats"
        ],
        [ExpenseCategories.Transport] =
        [
            "uber", "taxi", "fuel", "petrol", "shell", "bp", "caltex", "parking", "train", "bus", "opal"
        ],
        [ExpenseCategories.Shopping] =
        [
            "amazon", "target", "kmart", "myer", "david jones", "jb hi", "bunnings", "store", "shop"
        ],
        [ExpenseCategories.Bills] =
        [
            "telstra", "optus", "energy", "electricity", "water", "insurance", "internet", "bill"
        ]
    };

    public static string SuggestCategory(string merchant)
    {
        if (string.IsNullOrWhiteSpace(merchant))
        {
            return ExpenseCategories.Other;
        }

        foreach (var (category, keywords) in KeywordMap)
        {
            if (keywords.Any(keyword => merchant.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return category;
            }
        }

        return ExpenseCategories.Other;
    }
}
