using TabulateAI.Models;

namespace TabulateAI.Services;

public static class CategorySuggestionService
{
    private static readonly Dictionary<string, string[]> KeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [ExpenseCategories.Food] =
        [
            "cafe", "coffee", "restaurant", "mcdonald", "kfc", "subway", "pizza",
            "bakery", "starbucks", "uber eats", "doordash", "menulog", "deliveroo", "dining",
            "guzman", "gyg", "hungry jack"
        ],
        [ExpenseCategories.Groceries] =
        [
            "grocery", "woolworths", "coles", "aldi", "iga", "harris farm", "costco", "supermarket"
        ],
        [ExpenseCategories.Transport] =
        [
            "uber", "taxi", "fuel", "petrol", "gas", "gasoline", "station", "fuelmax",
            "shell", "bp", "caltex", "parking", "train", "bus", "opal", "toll", "rideshare",
            "supercheap auto"
        ],
        [ExpenseCategories.Shopping] =
        [
            "amazon", "target", "kmart", "myer", "david jones", "jb hi", "big w", "bigw",
            "harvey norman", "eb games", "ebgames", "apple store", "store", "shop", "retail"
        ],
        [ExpenseCategories.Home] =
        [
            "bunnings", "ikea", "freedom", "hardware", "furniture", "home depot", "officeworks"
        ],
        [ExpenseCategories.Bills] =
        [
            "telstra", "optus", "energy", "electricity", "water", "insurance", "internet", "bill", "council rates"
        ],
        [ExpenseCategories.Health] =
        [
            "pharmacy", "chemist", "chemist warehouse", "priceline", "doctor", "dental", "medical", "hospital"
        ],
        [ExpenseCategories.Entertainment] =
        [
            "netflix", "spotify", "cinema", "event", "ticket", "gaming", "steam", "disney", "hoyts", "eb games"
        ],
        [ExpenseCategories.Travel] =
        [
            "hotel", "airbnb", "flight", "airline", "qantas", "jetstar", "booking.com", "accommodation", "travel"
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
