using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IExpenseExportService _exportService;

    [ObservableProperty]
    private string _heroPeriodLabel = string.Empty;

    [ObservableProperty]
    private string _totalFormatted = "$0.00";

    [ObservableProperty]
    private string _summaryMeta = "0 receipts";

    [ObservableProperty]
    private string _selectedPeriod = "This month";

    [ObservableProperty]
    private List<CategoryBreakdownItem> _categories = [];

    [ObservableProperty]
    private List<StackedBarSegment> _stackedBarSegments = [];

    [ObservableProperty]
    private List<ReportLegendItem> _legendItems = [];

    [ObservableProperty]
    private string _userEmail = "Add your email in Settings";

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _topCategoryInsight = string.Empty;

    public ReportsViewModel(IReceiptRepository receiptRepository, IExpenseExportService exportService)
    {
        _receiptRepository = receiptRepository;
        _exportService = exportService;
    }

    public async Task InitializeAsync()
    {
        await LoadReportDataAsync();
    }

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        if (period == "Custom")
        {
            await Shell.Current.DisplayAlert(
                "Custom range",
                "Custom date ranges are not available yet. Use This month or Last month.",
                "OK");
            return;
        }

        SelectedPeriod = period;
        await LoadReportDataAsync();
    }

    [RelayCommand]
    private async Task PreviewCsvAsync()
    {
        if (IsExporting)
        {
            return;
        }

        var period = ReportPeriodHelper.Resolve(SelectedPeriod);
        var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);

        if (receipts.Count == 0)
        {
            await Shell.Current.DisplayAlert(
                "No receipts",
                $"There are no saved receipts for {period.Label}. Scan and save receipts first.",
                "OK");
            return;
        }

        await Shell.Current.GoToAsync($"exportpreview?Period={Uri.EscapeDataString(SelectedPeriod)}");
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (IsExporting)
        {
            return;
        }

        IsExporting = true;

        try
        {
            var period = ReportPeriodHelper.Resolve(SelectedPeriod);
            var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);

            if (receipts.Count == 0)
            {
                await Shell.Current.DisplayAlert(
                    "No receipts",
                    $"There are no saved receipts for {period.Label}. Scan and save receipts first.",
                    "OK");
                return;
            }

            var csv = _exportService.BuildCsv(receipts, period.Label);
            var filePath = await _exportService.SaveCsvAsync(csv, period.FileToken);

            var shareNow = await Shell.Current.DisplayAlert(
                "Export ready",
                $"Prepared {receipts.Count} receipt(s) as a CSV file.\n\nShare it to save to Downloads, open in Sheets, or email.",
                "Share",
                "Done");

            if (shareNow)
            {
                try
                {
                    await _exportService.ShareCsvAsync(filePath);
                }
                catch (Exception shareEx)
                {
                    await Shell.Current.DisplayAlert(
                        "Share failed",
                        $"The CSV was created but could not be shared: {shareEx.Message}",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Export failed", ex.Message, "OK");
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        await Shell.Current.DisplayAlert(
            "PDF export",
            "PDF export is not available yet. Use CSV export for now.",
            "OK");
    }

    [RelayCommand]
    private async Task EmailReportAsync()
    {
        if (UserEmail.StartsWith("Add your", StringComparison.Ordinal))
        {
            await Shell.Current.DisplayAlert(
                "Email",
                "Add your email address in Settings first.",
                "OK");
            return;
        }

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Expense report",
            Text = $"{HeroPeriodLabel}: {TotalFormatted}\n{SummaryMeta}"
        });
    }

    private async Task LoadReportDataAsync()
    {
        try
        {
            var period = ReportPeriodHelper.Resolve(SelectedPeriod);
            var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);

            HeroPeriodLabel = $"{period.Label} TOTAL";

            if (receipts.Count == 0)
            {
                TotalFormatted = 0m.ToString("C2");
                SummaryMeta = "0 receipts";
                Categories = [];
                StackedBarSegments = [];
                LegendItems = [];
                HasData = false;
                TopCategoryInsight = string.Empty;
                return;
            }

            var total = receipts.Sum(r => r.Amount);
            var summaries = receipts
                .GroupBy(r => r.Category)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    Total = g.Sum(r => r.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            TotalFormatted = total.ToString("C2");
            SummaryMeta = $"{receipts.Count} receipt{(receipts.Count == 1 ? string.Empty : "s")}";
            HasData = true;

            var top = summaries[0];
            var topShare = total > 0 ? top.Total / total : 0;
            TopCategoryInsight = $"{top.Category} was your top category at {topShare:P0} of spending";

            Categories = CategoryChartHelper.BuildBreakdown(summaries, total);
            StackedBarSegments = CategoryChartHelper.BuildStackedBar(summaries, total);
            LegendItems = CategoryChartHelper.BuildLegend(summaries);
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            HeroPeriodLabel = "TOTAL";
            TotalFormatted = 0m.ToString("C2");
            SummaryMeta = "Could not load report";
            Categories = [];
            StackedBarSegments = [];
            LegendItems = [];
            HasData = false;
            TopCategoryInsight = string.Empty;
        }
    }
}
