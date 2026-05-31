using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpendSmart.Services;

namespace SpendSmart.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IImageStorageService _imageStorageService;
    private readonly IOcrService _ocrService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Snap a receipt or pick one from your gallery.";

    public ScanViewModel(IImageStorageService imageStorageService, IOcrService ocrService)
    {
        _imageStorageService = imageStorageService;
        _ocrService = ocrService;
    }

    [RelayCommand]
    private async Task CapturePhotoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                StatusMessage = "Camera capture is not available on this device.";
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture receipt"
            });

            if (photo is null)
            {
                return;
            }

            await ProcessPhotoAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Camera is not supported on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Camera permission was denied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Capture failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Select receipt"
            });

            var photo = photos?.FirstOrDefault();

            if (photo is null)
            {
                return;
            }

            await ProcessPhotoAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Photo picker is not supported on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Storage permission was denied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Selection failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ManualEntryAsync()
    {
        await Shell.Current.GoToAsync("ReviewReceipt");
    }

    private async Task ProcessPhotoAsync(FileResult photo)
    {
        IsBusy = true;
        StatusMessage = "Reading receipt...";

        try
        {
            await using var stream = await photo.OpenReadAsync();
            var savedPath = await _imageStorageService.SaveReceiptImageAsync(stream, Path.GetExtension(photo.FileName));

            var extraction = await _ocrService.ExtractReceiptDataAsync(savedPath);
            var query = BuildReviewQuery(savedPath, extraction);
            await Shell.Current.GoToAsync($"ReviewReceipt?{query}");
            StatusMessage = "Snap a receipt or pick one from your gallery.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildReviewQuery(string imagePath, Models.OcrExtractionResult extraction)
    {
        var parts = new List<string>
        {
            $"ImagePath={Uri.EscapeDataString(imagePath)}"
        };

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

        if (!string.IsNullOrWhiteSpace(extraction.RawText))
        {
            parts.Add($"RawOcrText={Uri.EscapeDataString(extraction.RawText)}");
        }

        return string.Join("&", parts);
    }
}
