using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(PeriodSelection), "Period")]
public partial class ExportPreviewViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IExpenseExportService _exportService;

    private List<Receipt> _receipts = [];
    private ReportPeriodHelper.PeriodRange _periodRange = ReportPeriodHelper.Resolve("This month");

    [ObservableProperty]
    private string _periodSelection = "This month";

    [ObservableProperty]
    private string _periodLabel = string.Empty;

    [ObservableProperty]
    private string _totalFormatted = "$0.00";

    [ObservableProperty]
    private string _summaryMeta = string.Empty;

    [ObservableProperty]
    private List<ExportPreviewRow> _rows = [];

    [ObservableProperty]
    private bool _isExporting;

    public ExportPreviewViewModel(IReceiptRepository receiptRepository, IExpenseExportService exportService)
    {
        _receiptRepository = receiptRepository;
        _exportService = exportService;
    }

    partial void OnPeriodSelectionChanged(string value) => _ = LoadAsync();

    public Task InitializeAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task ShareCsvAsync()
    {
        if (IsExporting || _receipts.Count == 0)
        {
            return;
        }

        IsExporting = true;

        try
        {
            var csv = _exportService.BuildCsv(_receipts, _periodRange.Label);
            var filePath = await _exportService.SaveCsvAsync(csv, _periodRange.FileToken);
            await _exportService.ShareCsvAsync(filePath);
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

    private async Task LoadAsync()
    {
        _periodRange = ReportPeriodHelper.Resolve(PeriodSelection);
        PeriodLabel = _periodRange.Label;
        _receipts = await _receiptRepository.GetByDateRangeAsync(_periodRange.Start, _periodRange.End);

        if (_receipts.Count == 0)
        {
            TotalFormatted = 0m.ToString("C2");
            SummaryMeta = "No receipts";
            Rows = [];
            return;
        }

        var total = _receipts.Sum(r => r.Amount);
        TotalFormatted = total.ToString("C2");
        SummaryMeta = $"{_receipts.Count} receipt{(_receipts.Count == 1 ? string.Empty : "s")}";

        var rows = new List<ExportPreviewRow>
        {
            new()
            {
                IsHeader = true,
                Date = "Date",
                Merchant = "Merchant",
                Category = "Category",
                AmountDisplay = "Amount"
            }
        };

        rows.AddRange(_receipts
            .OrderByDescending(r => r.Date)
            .Select(r => new ExportPreviewRow
            {
                Date = r.Date.ToString("dd MMM yyyy"),
                Merchant = r.Merchant,
                Category = r.Category,
                Amount = r.Amount,
                AmountDisplay = r.Amount.ToString("C2")
            }));

        rows.Add(new ExportPreviewRow { IsDivider = true });

        foreach (var group in _receipts.GroupBy(r => r.Category).OrderByDescending(g => g.Sum(r => r.Amount)))
        {
            rows.Add(new ExportPreviewRow
            {
                IsSummary = true,
                Category = group.Key,
                Merchant = $"{group.Count()} receipt{(group.Count() == 1 ? string.Empty : "s")}",
                Amount = group.Sum(r => r.Amount),
                AmountDisplay = group.Sum(r => r.Amount).ToString("C2")
            });
        }

        Rows = rows;
    }
}
