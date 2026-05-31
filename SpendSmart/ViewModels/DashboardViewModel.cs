using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
using SpendSmart.Models;
using SpendSmart.Services;

namespace SpendSmart.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;

    private static readonly SKColor[] ChartColors =
    [
        SKColor.Parse("#2E7D32"),
        SKColor.Parse("#1565C0"),
        SKColor.Parse("#EF6C00"),
        SKColor.Parse("#6A1B9A"),
        SKColor.Parse("#546E7A")
    ];

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private string _monthLabel = string.Empty;

    [ObservableProperty]
    private List<CategorySummary> _categorySummaries = [];

    [ObservableProperty]
    private Chart? _categoryChart;

    [ObservableProperty]
    private bool _hasData;

    public DashboardViewModel(IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
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
    private async Task ShareSummaryAsync()
    {
        if (!HasData)
        {
            await Shell.Current.DisplayAlertAsync("Nothing to share", "Add receipts first to share a spending summary.", "OK");
            return;
        }

        var lines = CategorySummaries
            .Select(c => $"{c.Category}: {c.Total:C2} ({c.Count} receipts)")
            .Prepend($"{MonthLabel} total: {MonthlyTotal:C2}");

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "SpendSmart Summary",
            Text = string.Join(Environment.NewLine, lines)
        });
    }

    private async Task LoadDashboardAsync()
    {
        var now = DateTime.Now;
        MonthLabel = now.ToString("MMMM yyyy");
        MonthlyTotal = await _receiptRepository.GetMonthlyTotalAsync(now.Year, now.Month);
        CategorySummaries = await _receiptRepository.GetCategorySummariesAsync(now.Year, now.Month);
        HasData = CategorySummaries.Count > 0;
        CategoryChart = BuildChart(CategorySummaries);
    }

    private static Chart? BuildChart(IReadOnlyList<CategorySummary> summaries)
    {
        if (summaries.Count == 0)
        {
            return null;
        }

        var entries = summaries
            .Select((summary, index) => new ChartEntry((float)summary.Total)
            {
                Label = summary.Category,
                ValueLabel = summary.Total.ToString("C0"),
                Color = ChartColors[index % ChartColors.Length]
            })
            .ToList();

        return new DonutChart
        {
            Entries = entries,
            LabelTextSize = 28,
            BackgroundColor = SKColors.Transparent
        };
    }
}
