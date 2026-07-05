using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAppSettingsService _appSettings;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IBackupService _backupService;
    private readonly AiExtractionOptions _aiOptions;

    [ObservableProperty]
    private string _displayName = "User";

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _themeDescription = "Currently using light theme";

    [ObservableProperty]
    private string _userInitials = "U";

    [ObservableProperty]
    private string _appVersion = "1.0";

    [ObservableProperty]
    private string _ocrEngineLabel = "Gemini AI";

    [ObservableProperty]
    private string _storageLabel = "Local on this device";

    [ObservableProperty]
    private int _receiptCount;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _themeIcon = AppIcons.Moon;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private List<CategoryManageItem> _customCategories = [];

    [ObservableProperty]
    private List<BudgetEditorItem> _budgetEditors = [];

    public SettingsViewModel(
        IAppSettingsService appSettings,
        IReceiptRepository receiptRepository,
        IImageStorageService imageStorageService,
        IBackupService backupService,
        AiExtractionOptions aiOptions)
    {
        _appSettings = appSettings;
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
        _backupService = backupService;
        _aiOptions = aiOptions;
        _appSettings.SettingsChanged += (_, _) => SyncFromSettings();
    }

    public async Task InitializeAsync()
    {
        _appSettings.Load();
        SyncFromSettings();
        LoadCategoryAndBudgetEditors();
        AppVersion = AppInfo.Current.VersionString;
        OcrEngineLabel = _aiOptions.IsCloudEnabled ? "Gemini AI (cloud)" : "On-device fallback";

        try
        {
            var receipts = await _receiptRepository.GetAllAsync();
            ReceiptCount = receipts.Count;
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            ReceiptCount = 0;
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _appSettings.ToggleTheme();
    }

    public void SetDarkMode(bool enabled)
    {
        if (_appSettings.IsDarkMode == enabled)
        {
            return;
        }

        _appSettings.SetDarkMode(enabled);
    }

    [RelayCommand]
    private void SaveProfile()
    {
        _appSettings.DisplayName = DisplayName;
        _appSettings.Email = Email;
        SyncFromSettings();
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        var trimmed = NewCategoryName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            await Shell.Current.DisplayAlert("Category", "Enter a category name first.", "OK");
            return;
        }

        if (CategoryHelper.GetAllCategories().Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            await Shell.Current.DisplayAlert("Category", "That category already exists.", "OK");
            return;
        }

        CategoryHelper.AddCustomCategory(trimmed);
        NewCategoryName = string.Empty;
        LoadCategoryAndBudgetEditors();
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryManageItem item)
    {
        if (!item.IsCustom)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Delete category?",
            $"Remove \"{item.Name}\" from your custom categories?",
            "Delete",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        CategoryHelper.RemoveCustomCategory(item.Name);
        LoadCategoryAndBudgetEditors();
    }

    [RelayCommand]
    private async Task SaveBudgetsAsync()
    {
        foreach (var editor in BudgetEditors)
        {
            if (string.IsNullOrWhiteSpace(editor.BudgetText))
            {
                BudgetHelper.RemoveBudget(editor.Category);
                continue;
            }

            if (!decimal.TryParse(editor.BudgetText, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount) &&
                !decimal.TryParse(editor.BudgetText, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                await Shell.Current.DisplayAlert(
                    "Invalid budget",
                    $"Enter a valid amount for {editor.Category}.",
                    "OK");
                return;
            }

            BudgetHelper.SetBudget(editor.Category, amount);
        }

        await Shell.Current.DisplayAlert("Budgets saved", "Your monthly category budgets were updated.", "OK");
        LoadCategoryAndBudgetEditors();
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var zipPath = await _backupService.CreateBackupAsync();
            var shareNow = await Shell.Current.DisplayAlert(
                "Backup ready",
                "Your receipts, images, and settings were packaged into a backup file.",
                "Share backup",
                "Done");

            if (shareNow)
            {
                await _backupService.ShareBackupAsync(zipPath);
            }
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert("Backup failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose an Expensely backup",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.Android] = ["application/zip", ".zip"],
                    [DevicePlatform.iOS] = ["public.zip-archive"],
                    [DevicePlatform.WinUI] = [".zip"],
                    [DevicePlatform.MacCatalyst] = ["public.zip-archive"]
                })
            });

            if (result is null)
            {
                return;
            }

            var confirmed = await Shell.Current.DisplayAlert(
                "Restore backup?",
                "This replaces all local receipts, images, and settings on this device.",
                "Restore",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            IsBusy = true;
            await _backupService.RestoreBackupAsync(result.FullPath);
            _appSettings.Load();
            SyncFromSettings();
            LoadCategoryAndBudgetEditors();

            var receipts = await _receiptRepository.GetAllAsync();
            ReceiptCount = receipts.Count;

            await Shell.Current.DisplayAlert(
                "Restore complete",
                "Your backup was restored successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert("Restore failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearAllDataAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Delete all data?",
            "This permanently removes every receipt and saved image from this device.",
            "Delete all",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var receipts = await _receiptRepository.GetAllAsync();
            foreach (var receipt in receipts)
            {
                if (!string.IsNullOrWhiteSpace(receipt.ImagePath))
                {
                    await _imageStorageService.DeleteImageAsync(receipt.ImagePath);
                }

                if (!string.IsNullOrWhiteSpace(receipt.MerchantLogoPath))
                {
                    await _imageStorageService.DeleteImageAsync(receipt.MerchantLogoPath);
                }
            }

            await _receiptRepository.DeleteAllAsync();
            await _imageStorageService.ClearAllReceiptImagesAsync();
            ReceiptCount = 0;

            await Shell.Current.DisplayAlert(
                "Data cleared",
                "All receipts and images were removed from this device.",
                "OK");
        }
        catch (Exception ex)
        {
            App.WriteCrashLog(ex.ToString());
            await Shell.Current.DisplayAlert(
                "Could not clear data",
                "Something went wrong while deleting your local data.",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnDisplayNameChanged(string value)
    {
        UserInitials = BuildInitialsFromName(value);
    }

    private void LoadCategoryAndBudgetEditors()
    {
        CustomCategories = CategoryHelper.GetManageItems()
            .Where(item => item.IsCustom)
            .ToList();

        var budgets = BudgetHelper.GetAll();
        BudgetEditors = CategoryHelper.GetAllCategories()
            .Select(category => new BudgetEditorItem
            {
                Category = category,
                BudgetText = budgets.TryGetValue(category, out var amount)
                    ? amount.ToString("0.##", CultureInfo.CurrentCulture)
                    : string.Empty
            })
            .ToList();
    }

    private void SyncFromSettings()
    {
        DisplayName = _appSettings.DisplayName;
        Email = _appSettings.Email;
        IsDarkMode = _appSettings.IsDarkMode;
        ThemeIcon = _appSettings.ThemeIcon;
        ThemeDescription = _appSettings.IsDarkMode
            ? "Currently using dark theme"
            : "Currently using light theme";
        UserInitials = _appSettings.UserInitials;
    }

    private static string BuildInitialsFromName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "U";
        }

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0][..2].ToUpperInvariant()
                : parts[0][0].ToString().ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
