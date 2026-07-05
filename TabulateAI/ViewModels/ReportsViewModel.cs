using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using System.Globalization;

using TabulateAI.Helpers;

using TabulateAI.Models;

using TabulateAI.Services;



namespace TabulateAI.ViewModels;



public partial class ReportsViewModel : ObservableObject

{

    private readonly IReceiptRepository _receiptRepository;

    private readonly IExpenseExportService _exportService;

    private readonly IAppSettingsService _appSettings;



    [ObservableProperty]

    private string _heroPeriodLabel = string.Empty;



    [ObservableProperty]

    private string _totalFormatted = "$0.00";



    [ObservableProperty]

    private string _summaryMeta = "0 receipts";



    [ObservableProperty]

    private string _selectedPeriod = "This month";



    [ObservableProperty]

    private DateTime _customStartDate = DateTime.Today.AddDays(-30);



    [ObservableProperty]

    private DateTime _customEndDate = DateTime.Today;



    [ObservableProperty]

    private List<CategoryBreakdownItem> _categories = [];



    [ObservableProperty]

    private List<StackedBarSegment> _stackedBarSegments = [];



    [ObservableProperty]

    private List<ReportLegendItem> _legendItems = [];



    [ObservableProperty]

    private List<CategoryBudgetStatus> _budgetStatuses = [];



    [ObservableProperty]

    private string _userEmail = "Add your email in Settings";



    [ObservableProperty]

    private bool _isExporting;



    [ObservableProperty]

    private bool _hasData;



    [ObservableProperty]

    private bool _hasBudgets;



    [ObservableProperty]

    private string _topCategoryInsight = string.Empty;



    public bool IsCustomPeriod => SelectedPeriod == "Custom";



    public ReportsViewModel(

        IReceiptRepository receiptRepository,

        IExpenseExportService exportService,

        IAppSettingsService appSettings)

    {

        _receiptRepository = receiptRepository;

        _exportService = exportService;

        _appSettings = appSettings;

        _appSettings.SettingsChanged += (_, _) =>
        {
            SyncEmail();
            OnPropertyChanged(nameof(SelectedPeriod));
        };

        SyncEmail();

    }



    public async Task InitializeAsync()

    {

        _appSettings.Load();

        CustomStartDate = _appSettings.CustomReportStart;

        CustomEndDate = _appSettings.CustomReportEnd;

        SyncEmail();

        await LoadReportDataAsync();

    }



    partial void OnSelectedPeriodChanged(string value)

    {

        OnPropertyChanged(nameof(IsCustomPeriod));

    }



    [RelayCommand]

    private async Task SelectPeriodAsync(string period)

    {

        SelectedPeriod = period;



        if (period == "Custom")

        {

            CustomStartDate = _appSettings.CustomReportStart;

            CustomEndDate = _appSettings.CustomReportEnd;

        }



        await LoadReportDataAsync();

    }



    [RelayCommand]

    private async Task ApplyCustomRangeAsync()

    {

        if (CustomStartDate.Date > CustomEndDate.Date)

        {

            await Shell.Current.DisplayAlert(

                "Invalid range",

                "Start date must be on or before the end date.",

                "OK");

            return;

        }



        _appSettings.CustomReportStart = CustomStartDate;

        _appSettings.CustomReportEnd = CustomEndDate;

        SelectedPeriod = "Custom";

        await LoadReportDataAsync();

    }



    [RelayCommand]

    private async Task PreviewCsvAsync()

    {

        if (IsExporting)

        {

            return;

        }



        var period = GetSelectedPeriod();

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

            var period = GetSelectedPeriod();

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
        if (IsExporting)
        {
            return;
        }

        IsExporting = true;

        try
        {
            var period = GetSelectedPeriod();
            var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);

            if (receipts.Count == 0)
            {
                await Shell.Current.DisplayAlert(
                    "No receipts",
                    $"There are no saved receipts for {period.Label}. Scan and save receipts first.",
                    "OK");
                return;
            }

            var filePath = await _exportService.SavePdfAsync(receipts, period.Label, period.FileToken);

            var shareNow = await Shell.Current.DisplayAlert(
                "PDF ready",
                $"Prepared {receipts.Count} receipt(s) as a PDF report.\n\nShare it to save or email for reimbursement.",
                "Share",
                "Done");

            if (shareNow)
            {
                try
                {
                    await _exportService.SharePdfAsync(filePath);
                }
                catch (Exception shareEx)
                {
                    await Shell.Current.DisplayAlert(
                        "Share failed",
                        $"The PDF was created but could not be shared: {shareEx.Message}",
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

        if (!HasData)
        {
            var period = GetSelectedPeriod();
            await Shell.Current.DisplayAlert(
                "No receipts",
                $"There are no saved receipts for {period.Label}. Scan and save receipts first.",
                "OK");
            return;
        }

        await Shell.Current.GoToAsync($"emailreportpreview?Period={Uri.EscapeDataString(SelectedPeriod)}");
    }



    private async Task LoadReportDataAsync()

    {

        try

        {

            var period = GetSelectedPeriod();

            var receipts = await _receiptRepository.GetByDateRangeAsync(period.Start, period.End);



            HeroPeriodLabel = $"{period.Label} TOTAL";



            if (receipts.Count == 0)

            {

                TotalFormatted = 0m.ToString("C2");

                SummaryMeta = "0 receipts";

                Categories = [];

                StackedBarSegments = [];

                LegendItems = [];

                BudgetStatuses = [];

                HasBudgets = false;

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

            BudgetStatuses = BudgetHelper.BuildStatus(summaries);

            HasBudgets = BudgetStatuses.Count > 0;

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

            BudgetStatuses = [];

            HasBudgets = false;

            HasData = false;

            TopCategoryInsight = string.Empty;

        }

    }



    private ReportPeriodHelper.PeriodRange GetSelectedPeriod() =>

        ReportPeriodHelper.Resolve(

            SelectedPeriod,

            SelectedPeriod == "Custom" ? CustomStartDate : null,

            SelectedPeriod == "Custom" ? CustomEndDate : null);



    private void SyncEmail()

    {

        UserEmail = string.IsNullOrWhiteSpace(_appSettings.Email)

            ? "Add your email in Settings"

            : _appSettings.Email;

    }

}


