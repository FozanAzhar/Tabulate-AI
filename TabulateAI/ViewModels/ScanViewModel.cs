using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Snap a receipt or pick one from your gallery.";

    public ScanViewModel(IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
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
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select receipt"
            });

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

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    [RelayCommand]
    private async Task ToggleFlashAsync()
    {
        await Shell.Current.DisplayAlert("Flash", "Flash toggled.", "OK");
    }

    private async Task ProcessPhotoAsync(FileResult photo)
    {
        IsBusy = true;
        StatusMessage = "Saving receipt image...";

        try
        {
            await using var stream = await photo.OpenReadAsync();
            var savedPath = await _imageStorageService.SaveReceiptImageAsync(stream, Path.GetExtension(photo.FileName));
            await Shell.Current.GoToAsync($"Processing?ImagePath={Uri.EscapeDataString(savedPath)}");
            StatusMessage = "Snap a receipt or pick one from your gallery.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not read receipt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
