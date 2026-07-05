using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _infoStripText = "Smart extract on";

    public ScanViewModel(IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
    }

    [RelayCommand]
    private async Task CaptureAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Camera", "Camera capture is not available on this device.", "OK");
                return;
            }

            if (!await MediaPermissionHelper.EnsureCameraAsync())
            {
                await Shell.Current.DisplayAlert("Camera", "Camera permission is required to capture receipts.", "OK");
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
            await Shell.Current.DisplayAlert("Camera", "Camera is not supported on this device.", "OK");
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Camera", "Camera permission was denied.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Camera", $"Capture failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PickGalleryAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            var photo = await MediaPermissionHelper.PickReceiptImageAsync();

            if (photo is null)
            {
                return;
            }

            await ProcessPhotoAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Gallery", "Photo picker is not supported on this device.", "OK");
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Gallery", "Storage permission was denied.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Gallery", $"Selection failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await AppNavigation.GoDashboardAsync();
    }

    [RelayCommand]
    private async Task ManualEntryAsync()
    {
        await AppNavigation.GoManualExpenseAsync();
    }

    [RelayCommand]
    private void ToggleFlash()
    {
        // Flash hardware control is platform-specific; no-op until implemented.
    }

    public void ResetInteractionState() => IsBusy = false;

    private async Task ProcessPhotoAsync(FileResult photo)
    {
        IsBusy = true;

        try
        {
            await using var stream = await photo.OpenReadAsync();
            var extension = Path.GetExtension(photo.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            var savedPath = await _imageStorageService.SaveReceiptImageAsync(stream, extension);

            // Go to processing immediately — GPS runs there in parallel with OCR.
            IsBusy = false;
            await Shell.Current.GoToAsync($"processing?ImagePath={Uri.EscapeDataString(savedPath)}");
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert("Receipt", $"Could not read receipt: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
