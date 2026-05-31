using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private string _monthLabel = "May 2026";

    [ObservableProperty]
    private string _greeting = "Good morning, FA";

    [ObservableProperty]
    private string _userInitials = "FA";

    [ObservableProperty]
    private int _receiptCount;

    [ObservableProperty]
    private string _weeklyTotalFormatted = "$312";

    [ObservableProperty]
    private int _weeklyReceiptCount = 6;

    [ObservableProperty]
    private string _topCategory = "Grocery";

    [ObservableProperty]
    private string _topCategoryShare = "40% of spend";

    [ObservableProperty]
    private int _pendingCount = 2;

    [ObservableProperty]
    private string _trendLabel = "↑ 12% vs Apr";

    [ObservableProperty]
    private string _budgetLabel = "Budget: $1,500";

    [ObservableProperty]
    private List<ReceiptDisplayItem> _recentReceipts = [];

    [ObservableProperty]
    private bool _hasData = true;

    [ObservableProperty]
    private bool _showEmptyState;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public DashboardViewModel(IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
        var hour = DateTime.Now.Hour;
        var timeGreeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        Greeting = $"{timeGreeting}, FA";
    }

    public async Task InitializeAsync()
    {
        await LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task ScanReceiptAsync()
    {
        await Shell.Current.GoToAsync("//ScanPage");
    }

    [RelayCommand]
    private async Task OpenReceiptAsync(ReceiptDisplayItem item)
    {
        if (item.Id > 0)
        {
            await Shell.Current.GoToAsync($"ReviewReceipt?ReceiptId={item.Id}");
        }
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            var now = DateTime.Now;
            MonthLabel = now.ToString("MMMM yyyy");
            var monthlyTotal = await _receiptRepository.GetMonthlyTotalAsync(now.Year, now.Month);
            var allReceipts = await _receiptRepository.GetAllAsync();
            var monthReceipts = allReceipts
                .Where(r => r.Date.Year == now.Year && r.Date.Month == now.Month)
                .OrderByDescending(r => r.Date)
                .ToList();

            if (monthReceipts.Count == 0)
            {
                ApplySampleData();
            ShowEmptyState = false;
            HasData = true;
            return;
        }

        MonthlyTotal = monthlyTotal;
            ReceiptCount = monthReceipts.Count;
            RecentReceipts = monthReceipts.Take(3).Select(ReceiptDisplayHelper.ToDisplayItem).ToList();
            HasData = true;
            ShowEmptyState = false;

            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var weekReceipts = monthReceipts.Where(r => r.Date >= weekStart).ToList();
            WeeklyTotalFormatted = weekReceipts.Sum(r => r.Amount).ToString("C0");
            WeeklyReceiptCount = weekReceipts.Count;

            var summaries = await _receiptRepository.GetCategorySummariesAsync(now.Year, now.Month);
            if (summaries.Count > 0)
            {
                var top = summaries.OrderByDescending(s => s.Total).First();
                TopCategory = top.Category;
                TopCategoryShare = monthlyTotal > 0
                    ? $"{(int)((top.Total / monthlyTotal) * 100)}% of spend"
                    : "0% of spend";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load dashboard data.";
            ApplySampleData();
            System.Diagnostics.Debug.WriteLine($"Dashboard load failed: {ex}");
        }
    }

    private void ApplySampleData()
    {
        MonthlyTotal = 1284.60m;
        ReceiptCount = 34;
        RecentReceipts = ReceiptDisplayHelper.GetSampleReceipts();
        HasData = true;
        ShowEmptyState = false;
    }
}
