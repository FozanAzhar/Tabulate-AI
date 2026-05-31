using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class ReceiptDisplayHelper
{
    public static (string Icon, Color Background, Color IconColor) GetIconStyle(string merchant, string category)
    {
        var key = $"{merchant} {category}".ToLowerInvariant();

        if (key.Contains("uber") || key.Contains("travel") || key.Contains("transport"))
            return (IconGlyphs.Car, Color.FromArgb("#FFF8EC"), Color.FromArgb("#C8922A"));

        if (key.Contains("coffee") || key.Contains("pablo") || key.Contains("food"))
            return (IconGlyphs.Coffee, Color.FromArgb("#E8F2F9"), Color.FromArgb("#004080"));

        if (key.Contains("pharmacy") || key.Contains("health") || key.Contains("chemist"))
            return (IconGlyphs.FirstAid, Color.FromArgb("#F0F4F8"), Color.FromArgb("#003058"));

        if (key.Contains("airport") || key.Contains("flight"))
            return (IconGlyphs.Plane, Color.FromArgb("#FFF8EC"), Color.FromArgb("#C8922A"));

        if (key.Contains("jb") || key.Contains("office") || key.Contains("electronics"))
            return (IconGlyphs.Laptop, Color.FromArgb("#E8F2F9"), Color.FromArgb("#003058"));

        if (key.Contains("grocery") || key.Contains("woolworth") || key.Contains("coles") || key.Contains("shopping"))
            return (IconGlyphs.ShoppingCart, Color.FromArgb("#E8F2F9"), Color.FromArgb("#003058"));

        return (IconGlyphs.Receipt, Color.FromArgb("#E8F2F9"), Color.FromArgb("#003058"));
    }

    public static ReceiptDisplayItem ToDisplayItem(Receipt receipt)
    {
        var (icon, bg, iconColor) = GetIconStyle(receipt.Merchant, receipt.Category);
        return new ReceiptDisplayItem
        {
            Id = receipt.Id,
            Name = receipt.Merchant,
            Meta = $"{receipt.Category} · {receipt.Date:dd MMM}",
            Amount = receipt.Amount,
            Icon = icon,
            IconBackground = bg,
            IconColor = iconColor,
            Category = receipt.Category,
            Date = receipt.Date
        };
    }

    public static List<ReceiptDisplayItem> GetSampleReceipts() =>
    [
        new ReceiptDisplayItem
        {
            Name = "Woolworths",
            Meta = "Grocery · 11 May",
            Amount = 84.60m,
            Icon = IconGlyphs.ShoppingCart,
            IconBackground = Color.FromArgb("#E8F2F9"),
            IconColor = Color.FromArgb("#003058"),
            Category = "Grocery",
            Date = new DateTime(2026, 5, 11)
        },
        new ReceiptDisplayItem
        {
            Name = "Uber Trip",
            Meta = "Travel · 10 May",
            Amount = 32.10m,
            Icon = IconGlyphs.Car,
            IconBackground = Color.FromArgb("#FFF8EC"),
            IconColor = Color.FromArgb("#C8922A"),
            Category = "Travel",
            Date = new DateTime(2026, 5, 10)
        },
        new ReceiptDisplayItem
        {
            Name = "Pablo & Rusty's",
            Meta = "Food · 10 May",
            Amount = 11.50m,
            Icon = IconGlyphs.Coffee,
            IconBackground = Color.FromArgb("#E8F2F9"),
            IconColor = Color.FromArgb("#004080"),
            Category = "Food",
            Date = new DateTime(2026, 5, 10)
        }
    ];
}
