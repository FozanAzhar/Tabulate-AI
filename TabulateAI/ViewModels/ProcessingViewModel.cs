using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(ImageBase64), "ImageBase64")]
[QueryProperty(nameof(LocalPath), "LocalPath")]
[QueryProperty(nameof(ImagePath), "ImagePath")]
[QueryProperty(nameof(LocationAddress), "Address")]
[QueryProperty(nameof(LatitudeText), "Latitude")]
[QueryProperty(nameof(LongitudeText), "Longitude")]
public partial class ProcessingViewModel : ObservableObject
{
    private readonly IOcrService _ocrService;
    private readonly ILocationCaptureService _locationCaptureService;
    private readonly PendingReceiptContext _pendingReceipt;
    private string _reviewQuery = string.Empty;

    [ObservableProperty]
    private string _imageBase64 = string.Empty;

    [ObservableProperty]
    private string _localPath = string.Empty;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _locationAddress = string.Empty;

    [ObservableProperty]
    private string _latitudeText = string.Empty;

    [ObservableProperty]
    private string _longitudeText = string.Empty;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private string _statusText = "Starting extraction...";

    [ObservableProperty]
    private string _previewTotalFormatted = "—";

    [ObservableProperty]
    private bool _step1Done;

    [ObservableProperty]
    private bool _step2Done;

    [ObservableProperty]
    private bool _step3InProgress = true;

    [ObservableProperty]
    private double _step3Opacity = 0.5;

    public ProcessingViewModel(
        IOcrService ocrService,
        ILocationCaptureService locationCaptureService,
        PendingReceiptContext pendingReceipt)
    {
        _ocrService = ocrService;
        _locationCaptureService = locationCaptureService;
        _pendingReceipt = pendingReceipt;
    }

    public async Task StartProcessingAsync()
    {
        Step1Done = false;
        Step2Done = false;
        Step3InProgress = true;
        Step3Opacity = 0.5;
        IsComplete = false;
        PreviewTotalFormatted = "—";
        StatusText = "Preparing receipt...";

        var sourcePath = await ResolveImagePathAsync();
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            StatusText = "No receipt image provided.";
            CompleteProcessing(null, sourcePath);
            return;
        }

        if (!File.Exists(sourcePath))
        {
            StatusText = "Receipt image file was not found.";
            CompleteProcessing(null, sourcePath);
            return;
        }

        var locationTask = NeedsLocationCapture()
            ? _locationCaptureService.TryCaptureCurrentLocationAsync()
            : Task.FromResult<CapturedLocation?>(null);

        try
        {
            StatusText = "Uploading to AI server...";
            var extraction = await _ocrService.ExtractReceiptDataAsync(sourcePath);

            await TryApplyLocationAsync(locationTask);

            Step1Done = !string.IsNullOrWhiteSpace(extraction.Merchant);
            StatusText = Step1Done ? "Date & time found" : "Extracting details...";
            Step2Done = extraction.Date.HasValue;

            if (extraction.Amount.HasValue)
            {
                PreviewTotalFormatted = extraction.Amount.Value.ToString("C2");
            }

            _reviewQuery = BuildReviewQuery(sourcePath, extraction);
            _pendingReceipt.SetExtras(extraction.LineItems, extraction.RawText);
            CompleteProcessing(extraction, sourcePath);
            await ViewResultsAsync();
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await TryApplyLocationAsync(locationTask);
            _reviewQuery = BuildFallbackReviewQuery(sourcePath);
            StatusText = "Extraction finished with limited data.";
            CompleteProcessing(null, sourcePath);
            await ViewResultsAsync();
        }
    }

    [RelayCommand]
    private async Task ViewResultsAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_reviewQuery))
            {
                await AppNavigation.GoReceiptDetailAsync(_reviewQuery);
            }
            else
            {
                await AppNavigation.GoDashboardAsync();
            }
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert(
                "Navigation error",
                "Could not open the receipt detail screen.",
                "OK");
            await AppNavigation.GoDashboardAsync();
        }
    }

    private void CompleteProcessing(OcrExtractionResult? extraction, string sourcePath)
    {
        if (extraction is not null)
        {
            if (!Step1Done && !string.IsNullOrWhiteSpace(extraction.Merchant))
            {
                Step1Done = true;
            }

            if (!Step2Done && extraction.Date.HasValue)
            {
                Step2Done = true;
            }
        }

        Step3InProgress = false;
        Step3Opacity = 1.0;
        StatusText = "Extraction complete";
        IsComplete = true;

        if (string.IsNullOrWhiteSpace(_reviewQuery) && !string.IsNullOrWhiteSpace(sourcePath))
        {
            _reviewQuery = BuildFallbackReviewQuery(sourcePath);
        }
    }

    private async Task<string> ResolveImagePathAsync()
    {
        if (!string.IsNullOrWhiteSpace(LocalPath))
        {
            return LocalPath;
        }

        if (!string.IsNullOrWhiteSpace(ImagePath))
        {
            return ImagePath;
        }

        if (string.IsNullOrWhiteSpace(ImageBase64))
        {
            return string.Empty;
        }

        try
        {
            var bytes = Convert.FromBase64String(ImageBase64);
            var path = Path.Combine(FileSystem.CacheDirectory, $"receipt_{Guid.NewGuid():N}.jpg");
            await File.WriteAllBytesAsync(path, bytes);
            LocalPath = path;
            return path;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string BuildReviewQuery(string imagePath, OcrExtractionResult extraction)
    {
        var parts = new List<string> { $"ImagePath={Uri.EscapeDataString(imagePath)}" };

        if (!string.IsNullOrWhiteSpace(extraction.Merchant))
        {
            parts.Add($"Merchant={Uri.EscapeDataString(extraction.Merchant)}");
        }

        if (extraction.Amount.HasValue)
        {
            parts.Add($"Amount={extraction.Amount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        if (extraction.Date.HasValue)
        {
            parts.Add($"Date={extraction.Date.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(extraction.SuggestedCategory))
        {
            parts.Add($"Category={Uri.EscapeDataString(extraction.SuggestedCategory)}");
        }

        if (!string.IsNullOrWhiteSpace(extraction.CustomCategory))
        {
            parts.Add($"CustomCategory={Uri.EscapeDataString(extraction.CustomCategory)}");
        }

        if (!string.IsNullOrWhiteSpace(extraction.Source))
        {
            parts.Add($"ExtractionSource={Uri.EscapeDataString(extraction.Source)}");
        }

        if (!string.IsNullOrWhiteSpace(extraction.PaymentMethod))
        {
            parts.Add($"PaymentMethod={Uri.EscapeDataString(extraction.PaymentMethod)}");
        }

        if (extraction.Confidence.HasValue)
        {
            parts.Add($"Confidence={extraction.Confidence.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        if (extraction.ValidationIssues.Count > 0)
        {
            parts.Add($"ValidationIssues={Uri.EscapeDataString(string.Join("|", extraction.ValidationIssues))}");
        }

        var address = !string.IsNullOrWhiteSpace(LocationAddress)
            ? LocationAddress
            : extraction.Location;

        LocationQueryHelper.AppendTo(parts, address, LatitudeText, LongitudeText);
        parts.Add("ReturnTo=scan");
        return string.Join("&", parts);
    }

    private string BuildFallbackReviewQuery(string imagePath)
    {
        var parts = new List<string> { $"ImagePath={Uri.EscapeDataString(imagePath)}" };
        LocationQueryHelper.AppendTo(parts, LocationAddress, LatitudeText, LongitudeText);
        parts.Add("ReturnTo=scan");
        return string.Join("&", parts);
    }

    private bool NeedsLocationCapture() =>
        string.IsNullOrWhiteSpace(LocationAddress)
        && string.IsNullOrWhiteSpace(LatitudeText)
        && string.IsNullOrWhiteSpace(LongitudeText);

    private async Task TryApplyLocationAsync(Task<CapturedLocation?> locationTask)
    {
        if (!NeedsLocationCapture())
        {
            return;
        }

        try
        {
            var location = locationTask.IsCompleted
                ? await locationTask
                : await locationTask.WaitAsync(TimeSpan.FromSeconds(2));
            ApplyCapturedLocation(location);
        }
        catch (TimeoutException)
        {
            // GPS is optional — receipt OCR may still provide an address.
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
        }
    }

    private void ApplyCapturedLocation(CapturedLocation? location)
    {
        if (location is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(location.Address))
        {
            LocationAddress = location.Address.Trim();
        }

        LatitudeText = location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        LongitudeText = location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
