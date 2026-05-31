using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(ImagePath), "ImagePath")]
public partial class ProcessingViewModel : ObservableObject
{
    private readonly IOcrService _ocrService;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private string _statusText = "Extracting line items...";

    [ObservableProperty]
    private List<ProcessingStep> _steps =
    [
        new() { Label = "Store name extracted", Status = ProcessingStepStatus.Done },
        new() { Label = "Date & time found", Status = ProcessingStepStatus.Done },
        new() { Label = "Reading line items...", Status = ProcessingStepStatus.InProgress }
    ];

    public ProcessingViewModel(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    public async Task StartProcessingAsync()
    {
        _ = AnimateProgressAsync();

        if (string.IsNullOrWhiteSpace(ImagePath))
        {
            IsComplete = true;
            return;
        }

        try
        {
            var extraction = await _ocrService.ExtractReceiptDataAsync(ImagePath);
            var query = BuildReviewQuery(ImagePath, extraction);
            await MainThread.InvokeOnMainThreadAsync(() => IsComplete = true);
            _reviewQuery = query;
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsComplete = true);
            _reviewQuery = $"ImagePath={Uri.EscapeDataString(ImagePath)}";
        }
    }

    private string _reviewQuery = string.Empty;

    [RelayCommand]
    private async Task ViewExtractedDataAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_reviewQuery))
            {
                await Shell.Current.GoToAsync($"ReviewReceipt?{_reviewQuery}");
            }
            else
            {
                await Shell.Current.GoToAsync("ReviewReceipt");
            }
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert(
                "Navigation error",
                "Could not open the receipt detail screen. Try again from the History tab.",
                "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task AnimateProgressAsync()
    {
        while (!IsComplete)
        {
            Progress = 0.2;
            await ProgressBarAnimate();
            if (IsComplete) break;
        }
    }

    private async Task ProgressBarAnimate()
    {
        const int steps = 40;
        for (var i = 0; i <= steps && !IsComplete; i++)
        {
            Progress = 0.2 + (0.7 * i / steps);
            await Task.Delay(50);
        }
    }

    private static string BuildReviewQuery(string imagePath, OcrExtractionResult extraction)
    {
        var parts = new List<string> { $"ImagePath={Uri.EscapeDataString(imagePath)}" };

        if (!string.IsNullOrWhiteSpace(extraction.Merchant))
            parts.Add($"Merchant={Uri.EscapeDataString(extraction.Merchant)}");

        if (extraction.Amount.HasValue)
            parts.Add($"Amount={extraction.Amount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        if (extraction.Date.HasValue)
            parts.Add($"Date={extraction.Date.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(extraction.SuggestedCategory))
            parts.Add($"Category={Uri.EscapeDataString(extraction.SuggestedCategory)}");

        if (!string.IsNullOrWhiteSpace(extraction.Source))
            parts.Add($"ExtractionSource={Uri.EscapeDataString(extraction.Source)}");

        if (extraction.Confidence.HasValue)
            parts.Add($"Confidence={extraction.Confidence.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        if (extraction.ValidationIssues.Count > 0)
            parts.Add($"ValidationIssues={Uri.EscapeDataString(string.Join("|", extraction.ValidationIssues))}");

        return string.Join("&", parts);
    }
}
