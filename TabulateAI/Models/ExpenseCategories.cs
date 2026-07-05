namespace TabulateAI.Models;

public static class ExpenseCategories
{
    public const string Food = "Food";
    public const string Groceries = "Groceries";
    public const string Transport = "Transport";
    public const string Shopping = "Shopping";
    public const string Home = "Home";
    public const string Bills = "Bills";
    public const string Health = "Health";
    public const string Entertainment = "Entertainment";
    public const string Travel = "Travel";
    public const string Other = "Other";

    public static IReadOnlyList<string> All { get; } =
    [
        Food,
        Groceries,
        Transport,
        Shopping,
        Home,
        Bills,
        Health,
        Entertainment,
        Travel,
        Other
    ];
}
