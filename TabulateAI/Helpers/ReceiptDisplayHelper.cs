using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class ReceiptDisplayHelper
{
    public static (string IconSource, Color Background) GetIconStyle(string category)
    {
        return category switch
        {
            ExpenseCategories.Food => (AppIcons.Food, Color.FromArgb("#F5F3FF")),
            ExpenseCategories.Groceries => (AppIcons.Groceries, Color.FromArgb("#EDE9FE")),
            ExpenseCategories.Transport => (AppIcons.Transport, Color.FromArgb("#FFFBEB")),
            ExpenseCategories.Shopping => (AppIcons.Shopping, Color.FromArgb("#EFF6FF")),
            ExpenseCategories.Home => (AppIcons.Home, Color.FromArgb("#EEF2FF")),
            ExpenseCategories.Bills => (AppIcons.Bills, Color.FromArgb("#ECFDF5")),
            ExpenseCategories.Health => (AppIcons.Health, Color.FromArgb("#FDF2F8")),
            ExpenseCategories.Entertainment => (AppIcons.Entertainment, Color.FromArgb("#FFF7ED")),
            ExpenseCategories.Travel => (AppIcons.Travel, Color.FromArgb("#F0F9FF")),
            _ => (AppIcons.Receipt, Color.FromArgb("#F4F4F5"))
        };
    }

    public static ReceiptDisplayItem ToDisplayItem(Receipt receipt)
    {
        var (iconSource, iconBackground, hasMerchantLogo) =
            ResolveDisplayIcon(receipt.MerchantLogoPath, receipt.Category);

        return new ReceiptDisplayItem
        {
            Id = receipt.Id,
            Name = receipt.Merchant,
            Meta = $"{receipt.Category} · {receipt.Date:dd MMM}",
            Amount = receipt.Amount,
            IconSource = iconSource,
            IconBackground = iconBackground,
            HasMerchantLogo = hasMerchantLogo,
            Category = receipt.Category,
            Date = receipt.Date
        };
    }

    public static (string IconSource, Color Background, bool HasMerchantLogo) ResolveDisplayIcon(
        string? merchantLogoPath,
        string category)
    {
        var hasMerchantLogo = !string.IsNullOrWhiteSpace(merchantLogoPath) &&
                              File.Exists(merchantLogoPath);

        if (hasMerchantLogo)
        {
            return (merchantLogoPath, Color.FromArgb("#FFFFFF"), true);
        }

        var (iconSource, background) = GetIconStyle(category);
        return (iconSource, background, false);
    }
}
