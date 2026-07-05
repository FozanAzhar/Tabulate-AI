namespace TabulateAI.Helpers;

public static class MediaPermissionHelper
{
    public static async Task<bool> EnsureCameraAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }

    public static async Task<FileResult?> PickReceiptImageAsync()
    {
#if ANDROID
        // Document picker browses Downloads/Pictures reliably on emulators; MediaPicker often shows empty.
        return await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select receipt",
            FileTypes = FilePickerFileType.Images
        });
#else
        if (!await EnsurePhotosAsync())
        {
            return null;
        }

        return await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
        {
            Title = "Select receipt"
        });
#endif
    }

    private static async Task<bool> EnsurePhotosAsync()
    {
        PermissionStatus status;

        if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 13)
        {
            status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            }
        }
        else
        {
            status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
            }
        }

        return status == PermissionStatus.Granted;
    }
}
