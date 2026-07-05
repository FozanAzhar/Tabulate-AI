using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IMerchantLogoService _merchantLogoService;

    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private string _heroPeriodLabel = string.Empty;

    [ObservableProperty]
    private string _greeting = "Good morning";

    [ObservableProperty]
    private string _userInitials = "FA";

    [ObservableProperty]
    private int _receiptCount;

    [ObservableProperty]
    private string _weeklyTotalFormatted = "$0";

    [ObservableProperty]
    private int _weeklyReceiptCount;

    [ObservableProperty]
    private string _topCategory = "—";

    [ObservableProperty]
    private string _topCategoryShare = string.Empty;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private string _trendLabel = string.Empty;

    [ObservableProperty]
    private bool _showTrendChip;

    [ObservableProperty]
    private List<ReceiptDisplayItem> _recentReceipts = [];

    [ObservableProperty]
    private List<CategoryBreakdownItem> _categoryBreakdown = [];

    [ObservableProperty]
    private List<StackedBarSegment> _stackedBarSegments = [];

    [ObservableProperty]
    private List<ReportLegendItem> _legendItems = [];

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private bool _showEmptyState = true;

    [ObservableProperty]
    private bool _hasCategoryBreakdown;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public DashboardViewModel(IReceiptRepository receiptRepository, IMerchantLogoService merchantLogoService)
    {
        _receiptRepository = receiptRepository;
        _merchantLogoService = merchantLogoService;
        Greeting = BuildGreeting();
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
    private async Task ScanAsync()
    {
        await Shell.Current.GoToAsync("//scan");
    }

    [RelayCommand]
    private async Task AddManualAsync()
    {
        await AppNavigation.GoManualExpenseAsync();
    }

    [RelayCommand]
    private async Task OpenReceiptAsync(ReceiptDisplayItem item)
    {
        if (item.Id > 0)
        {
            await Shell.Current.GoToAsync($"receiptdetail?ReceiptId={item.Id}&ReturnTo=dashboard");
        }
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            Greeting = BuildGreeting();

            var now = DateTime.Now;
            HeroPeriodLabel = $"{now:MMMM yyyy}".ToUpperInvariant() + " · TOTAL SPENT";

            var monthlyTotal = await _receiptRepository.GetMonthlyTotalAsync(now.Year, now.Month);
            var allReceipts = await _receiptRepository.GetAllAsync();
            await _merchantLogoService.BackfillMissingLogosAsync(allReceipts, _receiptRepository, 10);
            allReceipts = await _receiptRepository.GetAllAsync();
            var monthReceipts = allReceipts
                .Where(r => r.Date.Year == now.Year && r.Date.Month == now.Month)
                .OrderByDescending(r => r.Date)
                .ToList();

            if (monthReceipts.Count == 0)
            {
                TotalSpent = 0;
                ReceiptCount = 0;
                RecentReceipts = [];
                CategoryBreakdown = [];
                StackedBarSegments = [];
                LegendItems = [];
                HasCategoryBreakdown = false;
                WeeklyTotalFormatted = "$0";
                WeeklyReceiptCount = 0;
                TopCategory = "—";
                TopCategoryShare = string.Empty;
                PendingCount = 0;
                ShowTrendChip = false;
                HasData = false;
                ShowEmptyState = true;
                return;
            }

            TotalSpent = monthlyTotal;
            ReceiptCount = monthReceipts.Count;
            HasData = true;
            ShowEmptyState = false;

            var recent = monthReceipts.Take(3).Select(ReceiptDisplayHelper.ToDisplayItem).ToList();
            for (var i = 0; i < recent.Count; i++)
            {
                recent[i].ShowDivider = i < recent.Count - 1;
            }

            RecentReceipts = recent;

            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var weekReceipts = monthReceipts.Where(r => r.Date >= weekStart).ToList();
            WeeklyTotalFormatted = weekReceipts.Sum(r => r.Amount).ToString("C0");
            WeeklyReceiptCount = weekReceipts.Count;

            PendingCount = monthReceipts.Count(r =>
                string.IsNullOrWhiteSpace(r.Category) || r.Category == ExpenseCategories.Other);

            var summaries = await _receiptRepository.GetCategorySummariesAsync(now.Year, now.Month);
            if (summaries.Count > 0)
            {
                var top = summaries.OrderByDescending(s => s.Total).First();
                TopCategory = top.Category;
                TopCategoryShare = monthlyTotal > 0
                    ? $"{(int)((top.Total / monthlyTotal) * 100)}% of spend"
                    : string.Empty;
            }
            else
            {
                TopCategory = "—";
                TopCategoryShare = string.Empty;
            }

            CategoryBreakdown = CategoryChartHelper.BuildBreakdown(summaries, monthlyTotal);
            StackedBarSegments = CategoryChartHelper.BuildStackedBar(summaries, monthlyTotal);
            LegendItems = CategoryChartHelper.BuildLegend(summaries);
            HasCategoryBreakdown = CategoryBreakdown.Count > 0;

            var previousMonth = now.AddMonths(-1);
            var previousTotal = await _receiptRepository.GetMonthlyTotalAsync(previousMonth.Year, previousMonth.Month);
            if (previousTotal > 0)
            {
                var change = (int)Math.Round((monthlyTotal - previousTotal) / previousTotal * 100);
                var monthName = previousMonth.ToString("MMMM");
                TrendLabel = change >= 0
                    ? $"↑ {change}% vs {monthName}"
                    : $"↓ {Math.Abs(change)}% vs {monthName}";
                ShowTrendChip = true;
            }
            else
            {
                ShowTrendChip = false;
                TrendLabel = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load dashboard data.";
            HasData = false;
            ShowEmptyState = true;
            System.Diagnostics.Debug.WriteLine($"Dashboard load failed: {ex}");
        }
    }

    private static string BuildGreeting()
    {
        var hour = DateTime.Now.Hour;
        var timeGreeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };
        return timeGreeting;
    }
}
