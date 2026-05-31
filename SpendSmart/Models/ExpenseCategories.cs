namespace SpendSmart.Models;

public static class ExpenseCategories
{
    public const string Food = "Food";
    public const string Transport = "Transport";
    public const string Shopping = "Shopping";
    public const string Bills = "Bills";
    public const string Other = "Other";

    public static IReadOnlyList<string> All { get; } =
    [
        Food,
        Transport,
        Shopping,
        Bills,
        Other
    ];
}
