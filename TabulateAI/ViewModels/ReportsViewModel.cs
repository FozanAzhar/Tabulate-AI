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
    private string _monthLabel = "MAY 2026";

    [ObservableProperty]
    private string _totalFormatted = "$1,284.60";

    [ObservableProperty]
    private string _summaryMeta = "34 receipts · Budget: $1,500 · 14% remaining";

    [ObservableProperty]
    private string _selectedPeriod = "This Month";

    [ObservableProperty]
    private List<CategoryBreakdownItem> _categories = [];

    [ObservableProperty]
    private bool _isExporting;

    public ReportsViewModel(IReceiptRepository receiptRepository, IExpenseExportService exportService)
    {
        _receiptRepository = receiptRepository;
        _exportService = exportService;
        LoadSampleData();
    }

    public async Task InitializeAsync()
    {
        await LoadReportDataAsync(useSampleFallback: true);
    }

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period;
        await LoadReportDataAsync(useSampleFallback: true);
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
                    $"There are no saved receipts for {period.Label}. Scan and save receipts first, then export again.",
                    "OK");
                return;
            }

            var csv = _exportService.BuildCsv(receipts, period.Label);
            var filePath = await _exportService.SaveCsvAsync(csv, period.FileToken);

            var openFile = await Shell.Current.DisplayAlert(
                "Export complete",
                $"Saved {receipts.Count} receipt(s) to:\n{filePath}\n\nOpen the file now?",
                "Open",
                "Done");

            if (openFile)
            {
                try
                {
                    await _exportService.OpenCsvAsync(filePath);
                }
                catch
                {
                    await Shell.Current.DisplayAlert(
                        "Saved",
                        "The CSV was saved successfully. Open it from your Documents\\Expensely\\Exports folder.",
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
        await Shell.Current.DisplayAlert("Export", "PDF export would generate here.", "OK");
    }

    [RelayCommand]
    private async Task EmailReportAsync()
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Expense Report",
            Text = $"{MonthLabel} total: {TotalFormatted}"
        });
    }

    private async Task LoadReportDataAsync(bool useSampleFallback)
    {
        try
        {
            var period = ReportPeriodHelper.Resolve(SelectedPeriod);
            var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);

            if (receipts.Count == 0)
            {
                if (useSampleFallback)
                {
                    LoadSampleData();
                }
                else
                {
                    MonthLabel = period.Label;
                    TotalFormatted = 0m.ToString("C2");
                    SummaryMeta = "0 receipts";
                    Categories = [];
                }

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

            MonthLabel = period.Label;
            TotalFormatted = total.ToString("C2");
            SummaryMeta = $"{receipts.Count} receipts · Budget: $1,500 · 14% remaining";
            ApplyBreakdown(summaries, total);
        }
        catch
        {
            if (useSampleFallback)
            {
                LoadSampleData();
            }
        }
    }

    private void LoadSampleData()
    {
        Categories =
        [
            new CategoryBreakdownItem { Category = "Grocery", Amount = 514m, Percent = 0.40, BarWidth = 0.80, BarColor = Color.FromArgb("#003058"), LegendColor = Color.FromArgb("#C8922A") },
            new CategoryBreakdownItem { Category = "Travel", Amount = 321m, Percent = 0.25, BarWidth = 0.50, BarColor = Color.FromArgb("#C8922A"), LegendColor = Color.FromArgb("#80C8922A") },
            new CategoryBreakdownItem { Category = "Health", Amount = 257m, Percent = 0.20, BarWidth = 0.40, BarColor = Color.FromArgb("#B8D4E8"), LegendColor = Color.FromArgb("#80B8D4E8") },
            new CategoryBreakdownItem { Category = "Office", Amount = 128m, Percent = 0.10, BarWidth = 0.20, BarColor = Color.FromArgb("#004080"), LegendColor = Color.FromArgb("#4DB8D4E8") },
            new CategoryBreakdownItem { Category = "Other", Amount = 64m, Percent = 0.05, BarWidth = 0.10, BarColor = Color.FromArgb("#D0DCE8"), LegendColor = Color.FromArgb("#26FFFFFF") }
        ];
    }

    private void ApplyBreakdown(List<CategorySummary> summaries, decimal total)
    {
        var colors = new[]
        {
            Color.FromArgb("#003058"),
            Color.FromArgb("#C8922A"),
            Color.FromArgb("#B8D4E8"),
            Color.FromArgb("#004080"),
            Color.FromArgb("#D0DCE8")
        };

        Categories = summaries.Select((s, i) => new CategoryBreakdownItem
        {
            Category = s.Category,
            Amount = s.Total,
            Percent = total > 0 ? (double)(s.Total / total) : 0,
            BarWidth = total > 0 ? (double)(s.Total / total) : 0,
            BarColor = colors[i % colors.Length],
            LegendColor = colors[i % colors.Length]
        }).ToList();
    }
}
