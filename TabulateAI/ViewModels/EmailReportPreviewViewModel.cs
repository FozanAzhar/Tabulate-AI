using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(PeriodSelection), "Period")]
public partial class EmailReportPreviewViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IExpenseExportService _exportService;
    private readonly IAppSettingsService _appSettings;

    private List<Receipt> _receipts = [];
    private ReportPeriodHelper.PeriodRange _periodRange = ReportPeriodHelper.Resolve("This month");
    private string? _pdfFilePath;

    [ObservableProperty]
    private string _periodSelection = "This month";

    [ObservableProperty]
    private string _periodLabel = string.Empty;

    [ObservableProperty]
    private string _senderName = string.Empty;

    [ObservableProperty]
    private string _senderEmail = string.Empty;

    [ObservableProperty]
    private string _recipientEmail = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private string _attachmentLabel = string.Empty;

    [ObservableProperty]
    private string _previewFrom = string.Empty;

    [ObservableProperty]
    private string _previewTo = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasReceipts;

    public EmailReportPreviewViewModel(
        IReceiptRepository receiptRepository,
        IExpenseExportService exportService,
        IAppSettingsService appSettings)
    {
        _receiptRepository = receiptRepository;
        _exportService = exportService;
        _appSettings = appSettings;
    }

    partial void OnPeriodSelectionChanged(string value) => _ = LoadAsync();

    partial void OnRecipientEmailChanged(string value) => UpdatePreviewHeaders();

    partial void OnSubjectChanged(string value) => OnPropertyChanged(nameof(PreviewSubject));

    partial void OnBodyChanged(string value) => OnPropertyChanged(nameof(PreviewBodySnippet));

    public string PreviewSubject => string.IsNullOrWhiteSpace(Subject) ? "Expense report" : Subject;

    public string PreviewBodySnippet
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Body))
            {
                return "Your message will appear here.";
            }

            var firstLines = Body
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(3);

            return string.Join('\n', firstLines);
        }
    }

    public Task InitializeAsync() => LoadAsync();

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private void ResetTemplate()
    {
        if (_receipts.Count == 0)
        {
            return;
        }

        var draft = EmailReportTemplateHelper.Build(_receipts, _periodRange.Label, SenderName);
        Subject = draft.Subject;
        Body = draft.Body;
        AttachmentLabel = draft.AttachmentLabel;
        UpdatePreviewHeaders();
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        if (IsBusy || !HasReceipts || string.IsNullOrWhiteSpace(_pdfFilePath))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(RecipientEmail))
        {
            await Shell.Current.DisplayAlert(
                "Recipient required",
                "Enter the email address you want to send this report to.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Subject))
        {
            await Shell.Current.DisplayAlert(
                "Subject required",
                "Add a subject line before sending.",
                "OK");
            return;
        }

        IsBusy = true;

        try
        {
            await _exportService.SendEmailReportAsync(
                RecipientEmail,
                Subject.Trim(),
                Body.Trim(),
                _pdfFilePath);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Could not send email", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            _appSettings.Load();
            SenderName = _appSettings.DisplayName;
            SenderEmail = _appSettings.Email;
            PreviewFrom = string.IsNullOrWhiteSpace(SenderEmail)
                ? "Your email account"
                : SenderEmail;

            _periodRange = ReportPeriodHelper.Resolve(
                PeriodSelection,
                PeriodSelection == "Custom" ? _appSettings.CustomReportStart : null,
                PeriodSelection == "Custom" ? _appSettings.CustomReportEnd : null);
            PeriodLabel = _periodRange.Label;

            _receipts = await _receiptRepository.GetByDateRangeAsync(_periodRange.Start, _periodRange.End);
            HasReceipts = _receipts.Count > 0;

            if (!HasReceipts)
            {
                Subject = string.Empty;
                Body = "No receipts were found for this period.";
                AttachmentLabel = "No attachment";
                _pdfFilePath = null;
                UpdatePreviewHeaders();
                return;
            }

            _pdfFilePath = await _exportService.SavePdfAsync(_receipts, _periodRange.Label, _periodRange.FileToken);

            var draft = EmailReportTemplateHelper.Build(_receipts, _periodRange.Label, SenderName);
            Subject = draft.Subject;
            Body = draft.Body;
            AttachmentLabel = draft.AttachmentLabel;
            UpdatePreviewHeaders();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Could not prepare email", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdatePreviewHeaders()
    {
        PreviewTo = string.IsNullOrWhiteSpace(RecipientEmail) ? "Add recipient" : RecipientEmail.Trim();
        OnPropertyChanged(nameof(PreviewSubject));
        OnPropertyChanged(nameof(PreviewBodySnippet));
    }
}
