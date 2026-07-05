namespace TabulateAI.Helpers;

public static class AppNavigation
{
    public const string Dashboard = "//dashboard";

    public static Task GoDashboardAsync() =>
        Shell.Current.GoToAsync(Dashboard);

    public static async Task GoReceiptDetailAsync(string query)
    {
        await Shell.Current.GoToAsync(Dashboard);
        await Shell.Current.GoToAsync($"receiptdetail?{query}");
    }

    public static Task GoManualExpenseAsync() =>
        Shell.Current.GoToAsync("manualexpense");
}
