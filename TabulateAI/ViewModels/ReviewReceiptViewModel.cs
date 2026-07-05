using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(ReceiptId), "ReceiptId")]
[QueryProperty(nameof(ImagePath), "ImagePath")]
[QueryProperty(nameof(Merchant), "Merchant")]
[QueryProperty(nameof(AmountText), "Amount")]
[QueryProperty(nameof(DateText), "Date")]
[QueryProperty(nameof(Category), "Category")]
[QueryProperty(nameof(CustomCategory), "CustomCategory")]
[QueryProperty(nameof(Description), "Description")]
[QueryProperty(nameof(Address), "Address")]
[QueryProperty(nameof(LatitudeText), "Latitude")]
[QueryProperty(nameof(LongitudeText), "Longitude")]
[QueryProperty(nameof(RawOcrText), "RawOcrText")]
[QueryProperty(nameof(ExtractionSource), "ExtractionSource")]
[QueryProperty(nameof(ConfidenceText), "Confidence")]
[QueryProperty(nameof(ValidationIssuesText), "ValidationIssues")]
[QueryProperty(nameof(PaymentMethod), "PaymentMethod")]
[QueryProperty(nameof(LineItemsJson), "LineItemsJson")]
[QueryProperty(nameof(ReturnTo), "ReturnTo")]
public partial class ReviewReceiptViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IMerchantLogoService _merchantLogoService;
    private readonly PendingReceiptContext _pendingReceipt;

    private string _merchantSnapshot = string.Empty;
    private string _amountSnapshot = string.Empty;
    private DateTime _dateSnapshot = DateTime.Today;
    private string _categorySnapshot = ExpenseCategories.Other;
    private string _customCategorySnapshot = string.Empty;
    private string _descriptionSnapshot = string.Empty;
    private string _addressSnapshot = string.Empty;
    private double? _latitudeSnapshot;
    private double? _longitudeSnapshot;

    [ObservableProperty]
    private int _receiptId;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _merchant = string.Empty;

    [ObservableProperty]
    private string _amountText = string.Empty;

    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private DateTime _receiptDate = DateTime.Today;

    [ObservableProperty]
    private string _category = ExpenseCategories.Other;

    [ObservableProperty]
    private string _customCategory = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private double? _latitude;

    [ObservableProperty]
    private double? _longitude;

    public string LatitudeText
    {
        get => Latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        set
        {
            if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                Latitude = parsed;
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                Latitude = null;
            }
        }
    }

    public string LongitudeText
    {
        get => Longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        set
        {
            if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                Longitude = parsed;
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                Longitude = null;
            }
        }
    }

    [ObservableProperty]
    private string _paymentMethod = "—";

    [ObservableProperty]
    private string _rawOcrText = string.Empty;

    [ObservableProperty]
    private string _extractionSource = string.Empty;

    [ObservableProperty]
    private string _confidenceText = string.Empty;

    [ObservableProperty]
    private double _confidenceValue;

    [ObservableProperty]
    private string _validationIssuesText = string.Empty;

    [ObservableProperty]
    private string _returnTo = string.Empty;

    [ObservableProperty]
    private bool _showWarning;

    [ObservableProperty]
    private string _warningText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private ImageSource? _receiptImage;

    [ObservableProperty]
    private List<LineItem> _lineItems = [];

    [ObservableProperty]
    private string _lineItemsJson = string.Empty;

    [ObservableProperty]
    private bool _categoryFromExtraction;

    [ObservableProperty]
    private string _merchantLogoPath = string.Empty;

    [ObservableProperty]
    private string _heroLogoSource = AppIcons.Receipt;

    [ObservableProperty]
    private Color _heroLogoBackground = Color.FromArgb("#F4F4F5");

    [ObservableProperty]
    private bool _hasHeroMerchantLogo;

    private bool _hasExtractedDate;

    public ObservableCollection<CategoryOption> CategoryOptions { get; } = [];

    public string StoreName => Merchant;

    public string AddressDisplay =>
        string.IsNullOrWhiteSpace(Address) ? string.Empty : $"📍 {Address.Trim()}";

    public bool HasAddress => !string.IsNullOrWhiteSpace(Address);

    public bool HasLineItems => LineItems.Count > 0;

    public bool CanEditAmount => !HasLineItems;

    public bool ShowNoLineItemsMessage => !HasLineItems;

    public string LineItemsSectionTitle =>
        HasLineItems ? $"Line items ({LineItems.Count})" : "Line items";

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool IsExistingReceipt => ReceiptId > 0;

    public bool ShowViewActions => IsExistingReceipt && !IsEditing;

    public bool ShowEditActions => IsEditing;

    public bool HasReceiptTime => ReceiptDateHelper.HasMeaningfulTime(ReceiptDate);

    public string ReceiptDateDisplay => ReceiptDate.ToString("dd MMM yyyy");

    public string Total =>
        decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var amt)
            ? amt.ToString("C2")
            : decimal.TryParse(AmountText, out amt)
                ? amt.ToString("C2")
                : "—";

    public string ConfidenceScoreDisplay =>
        ConfidenceValue > 0 ? $"{(int)Math.Round(ConfidenceValue)}%" : "—";

    public Color ConfidenceScoreColor => ConfidenceValue switch
    {
        >= 85 => Color.FromArgb("#10B981"),
        >= 60 => Color.FromArgb("#F59E0B"),
        > 0 => Color.FromArgb("#EF4444"),
        _ => Color.FromArgb("#6B7280")
    };

    public Color WarningBannerBackground => ConfidenceValue < 60
        ? Color.FromArgb("#FEF2F2")
        : Color.FromArgb("#FFFBEB");

    public Color WarningBannerBorder => ConfidenceValue < 60
        ? Color.FromArgb("#FECACA")
        : Color.FromArgb("#F59E0B");

    public Color WarningBannerTextColor => ConfidenceValue < 60
        ? Color.FromArgb("#EF4444")
        : Color.FromArgb("#92400E");

    public ReviewReceiptViewModel(
        IReceiptRepository receiptRepository,
        IImageStorageService imageStorageService,
        IMerchantLogoService merchantLogoService,
        PendingReceiptContext pendingReceipt)
    {
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
        _merchantLogoService = merchantLogoService;
        _pendingReceipt = pendingReceipt;
    }

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowViewActions));
        OnPropertyChanged(nameof(ShowEditActions));
    }

    partial void OnReceiptIdChanged(int value)
    {
        OnPropertyChanged(nameof(IsExistingReceipt));
        OnPropertyChanged(nameof(ShowViewActions));
        OnPropertyChanged(nameof(ShowEditActions));

        if (value > 0)
        {
            IsEditing = false;
            _ = LoadReceiptAsync(value);
        }
        else
        {
            IsEditing = true;
            RefreshCategoryOptions();
        }
    }

    partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(HasDescription));

    partial void OnConfidenceTextChanged(string value)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var confidence))
        {
            ConfidenceValue = confidence <= 1 ? confidence * 100 : confidence;
        }

        UpdateConfidenceBanner();
        NotifyConfidenceDisplayProperties();
    }

    partial void OnConfidenceValueChanged(double value)
    {
        UpdateConfidenceBanner();
        NotifyConfidenceDisplayProperties();
    }

    partial void OnMerchantChanged(string value)
    {
        OnPropertyChanged(nameof(StoreName));
        if (ReceiptId == 0
            && !CategoryFromExtraction
            && !string.IsNullOrWhiteSpace(value)
            && string.IsNullOrWhiteSpace(CustomCategory)
            && Category == ExpenseCategories.Other)
        {
            ApplyCategory(CategorySuggestionService.SuggestCategory(value));
        }
    }

    partial void OnImagePathChanged(string value) => UpdateImagePreview(value);

    partial void OnDateTextChanged(string value)
    {
        var parsed = ReceiptDateHelper.Parse(value);
        if (parsed.HasValue)
        {
            ReceiptDate = parsed.Value;
            _hasExtractedDate = true;
        }
    }

    partial void OnReceiptDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(HasReceiptTime));
        OnPropertyChanged(nameof(ReceiptDateDisplay));
    }

    partial void OnAmountTextChanged(string value) => OnPropertyChanged(nameof(Total));

    partial void OnAddressChanged(string value)
    {
        OnPropertyChanged(nameof(AddressDisplay));
        OnPropertyChanged(nameof(HasAddress));
    }

    partial void OnLineItemsChanged(List<LineItem> value)
    {
        OnPropertyChanged(nameof(HasLineItems));
        OnPropertyChanged(nameof(CanEditAmount));
        OnPropertyChanged(nameof(ShowNoLineItemsMessage));
        OnPropertyChanged(nameof(LineItemsSectionTitle));
    }

    partial void OnLineItemsJsonChanged(string value)
    {
        if (ReceiptId > 0 || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var items = LineItemsHelper.Deserialize(value);
        if (items.Count == 0)
        {
            return;
        }

        SetLineItems(items);
    }

    partial void OnCategoryChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            CategoryFromExtraction = true;
        }

        SyncCategorySelection(value);
        UpdateHeroLogoDisplay();
    }

    partial void OnCustomCategoryChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (ReceiptId == 0)
            {
                CategoryFromExtraction = true;
            }

            if (!CategoryHelper.IsPresetCategory(Category) ||
                string.Equals(Category, ExpenseCategories.Other, StringComparison.OrdinalIgnoreCase))
            {
                Category = value.Trim();
            }
        }

        SyncCategorySelection(Category);
    }

    private void NotifyConfidenceDisplayProperties()
    {
        OnPropertyChanged(nameof(ConfidenceScoreDisplay));
        OnPropertyChanged(nameof(ConfidenceScoreColor));
        OnPropertyChanged(nameof(WarningBannerBackground));
        OnPropertyChanged(nameof(WarningBannerBorder));
        OnPropertyChanged(nameof(WarningBannerTextColor));
    }

    private void UpdateConfidenceBanner()
    {
        ShowWarning = ConfidenceValue > 0 && ConfidenceValue < 85;

        if (!ShowWarning)
        {
            WarningText = string.Empty;
            return;
        }

        WarningText = ConfidenceValue < 60
            ? "Low confidence — please verify all fields"
            : $"Some fields may need review — confidence: {(int)Math.Round(ConfidenceValue)}%";
    }

    private async Task LoadReceiptAsync(int id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return;
        }

        Merchant = receipt.Merchant;
        AmountText = receipt.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        ReceiptDate = receipt.Date;
        _hasExtractedDate = true;
        Description = receipt.Description;
        Address = receipt.LocationAddress;
        Latitude = receipt.Latitude;
        Longitude = receipt.Longitude;
        ImagePath = receipt.ImagePath;
        RawOcrText = receipt.RawOcrText;
        PaymentMethod = string.IsNullOrWhiteSpace(receipt.PaymentMethod) ? "—" : receipt.PaymentMethod;
        var loadedItems = LineItemsHelper.Deserialize(receipt.LineItemsJson);
        SetLineItems(loadedItems);

        if (CategoryHelper.IsPresetCategory(receipt.Category) ||
            CategoryHelper.GetCustomCategories().Contains(receipt.Category, StringComparer.OrdinalIgnoreCase))
        {
            CustomCategory = string.Empty;
            Category = receipt.Category;
        }
        else
        {
            CustomCategory = receipt.Category;
            Category = receipt.Category;
        }

        RefreshCategoryOptions();
        await EnsureMerchantLogoAsync(receipt);
    }

    private async Task EnsureMerchantLogoAsync(Receipt receipt)
    {
        MerchantLogoPath = receipt.MerchantLogoPath;

        if (string.IsNullOrWhiteSpace(MerchantLogoPath))
        {
            MerchantLogoPath = await _merchantLogoService.TryResolveLogoPathAsync(receipt.Merchant) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(MerchantLogoPath))
            {
                receipt.MerchantLogoPath = MerchantLogoPath;
                await _receiptRepository.SaveAsync(receipt);
            }
        }

        UpdateHeroLogoDisplay();
    }

    private void UpdateHeroLogoDisplay()
    {
        var category = ResolveCategory();
        var (iconSource, _, hasMerchantLogo) =
            ReceiptDisplayHelper.ResolveDisplayIcon(MerchantLogoPath, category);

        HeroLogoSource = iconSource;
        HeroLogoBackground = hasMerchantLogo
            ? Color.FromArgb("#FFFFFF")
            : Color.FromArgb("#26A78BFA");
        HasHeroMerchantLogo = hasMerchantLogo;
    }

    private void RefreshCategoryOptions()
    {
        var effectiveCategory = ResolveCategory();

        CategoryOptions.Clear();
        foreach (var name in CategoryHelper.GetAllCategories())
        {
            CategoryOptions.Add(new CategoryOption
            {
                Name = name,
                IsSelected = string.Equals(name, effectiveCategory, StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(CustomCategory)
            });
        }
    }

    private void SyncCategorySelection(string selected)
    {
        foreach (var option in CategoryOptions)
        {
            option.IsSelected = string.Equals(option.Name, selected, StringComparison.OrdinalIgnoreCase);
        }
    }

    private void ApplyCategory(string category)
    {
        CustomCategory = string.Empty;
        Category = category;
        SyncCategorySelection(category);
    }

    private string ResolveCategory()
    {
        if (!string.IsNullOrWhiteSpace(CustomCategory))
        {
            return CustomCategory.Trim();
        }

        return string.IsNullOrWhiteSpace(Category) ? ExpenseCategories.Other : Category.Trim();
    }

    private void SnapshotFields()
    {
        _merchantSnapshot = Merchant;
        _amountSnapshot = AmountText;
        _dateSnapshot = ReceiptDate;
        _categorySnapshot = Category;
        _customCategorySnapshot = CustomCategory;
        _descriptionSnapshot = Description;
        _addressSnapshot = Address;
        _latitudeSnapshot = Latitude;
        _longitudeSnapshot = Longitude;
    }

    private void RestoreSnapshot()
    {
        Merchant = _merchantSnapshot;
        AmountText = _amountSnapshot;
        ReceiptDate = _dateSnapshot;
        Category = _categorySnapshot;
        CustomCategory = _customCategorySnapshot;
        Description = _descriptionSnapshot;
        Address = _addressSnapshot;
        Latitude = _latitudeSnapshot;
        Longitude = _longitudeSnapshot;
        RefreshCategoryOptions();
    }

    private void UpdateImagePreview(string path)
    {
        ReceiptImage = !string.IsNullOrWhiteSpace(path) && File.Exists(path)
            ? ImageSource.FromFile(path)
            : null;
    }

    public void FinalizeLoadedState()
    {
        UpdateImagePreview(ImagePath);

        if (ReceiptId == 0)
        {
            IsEditing = true;
            ApplyExtractedCategory();
            ApplyPendingExtras();

            if (!_hasExtractedDate && string.IsNullOrWhiteSpace(DateText))
            {
                ShowWarning = true;
                WarningText = "No receipt date was extracted — please set the correct date before saving.";
            }

            RefreshCategoryOptions();

            if (LineItems.Count == 0 && !string.IsNullOrWhiteSpace(LineItemsJson))
            {
                var items = LineItemsHelper.Deserialize(LineItemsJson);
                SetLineItems(items);
            }
        }

        if (ReceiptId == 0 && string.IsNullOrWhiteSpace(PaymentMethod))
        {
            PaymentMethod = "—";
        }

        if (ReceiptId != 0 || !string.IsNullOrWhiteSpace(Merchant) || !string.IsNullOrWhiteSpace(AmountText))
        {
            return;
        }

        ShowWarning = true;
        WarningText = "No receipt details were extracted. Start TabulateAI.Api on your PC with AI keys configured, then scan again — or enter the store name and total manually before saving.";
        NotifyConfidenceDisplayProperties();
    }

    private void ApplyPendingExtras()
    {
        var (items, rawOcrText) = _pendingReceipt.ConsumeExtras();

        if (items.Count > 0)
        {
            SetLineItems(items);
        }

        if (string.IsNullOrWhiteSpace(RawOcrText) && !string.IsNullOrWhiteSpace(rawOcrText))
        {
            RawOcrText = rawOcrText;
        }
    }

    private void SetLineItems(List<LineItem> items)
    {
        LineItemsHelper.ApplyDividers(items);
        LineItems = items;
    }

    private void ApplyExtractedCategory()
    {
        if (!CategoryFromExtraction)
        {
            return;
        }

        var preset = Category?.Trim() ?? ExpenseCategories.Other;
        var custom = CustomCategory?.Trim() ?? string.Empty;

        if (!CategoryHelper.IsPresetCategory(preset) && string.IsNullOrWhiteSpace(custom))
        {
            custom = preset;
        }

        if (!string.IsNullOrWhiteSpace(custom))
        {
            CustomCategory = custom;

            if (CategoryHelper.IsPresetCategory(preset) &&
                !string.Equals(preset, ExpenseCategories.Other, StringComparison.OrdinalIgnoreCase))
            {
                Category = preset;
                SyncCategorySelection(preset);
                return;
            }

            Category = custom;
            SyncCategorySelection(custom);
            return;
        }

        if (CategoryHelper.IsPresetCategory(preset))
        {
            ApplyCategory(preset);
        }
    }

    [RelayCommand]
    private void SelectCategory(CategoryOption option)
    {
        CustomCategory = string.Empty;
        Category = option.Name;
        SyncCategorySelection(option.Name);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Merchant))
        {
            await Shell.Current.DisplayAlert("Missing merchant", "Please enter a merchant name.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) &&
            !decimal.TryParse(AmountText, out amount))
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid amount greater than zero.", "OK");
            return;
        }

        if (HasLineItems && ReceiptId > 0 &&
            decimal.TryParse(_amountSnapshot, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var snapshotAmount))
        {
            amount = snapshotAmount;
        }

        if (amount <= 0)
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid amount greater than zero.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            var category = ResolveCategory();
            CategoryHelper.AddCustomCategory(category);

            var receipt = ReceiptId > 0
                ? await _receiptRepository.GetByIdAsync(ReceiptId) ?? new Receipt()
                : new Receipt();

            receipt.Merchant = Merchant.Trim();
            receipt.Amount = amount;
            receipt.Date = ReceiptDate.Date;
            receipt.Category = category;
            receipt.Description = Description.Trim();
            receipt.LocationAddress = Address.Trim();
            receipt.Latitude = Latitude;
            receipt.Longitude = Longitude;
            receipt.ImagePath = ImagePath;
            receipt.RawOcrText = RawOcrText;
            receipt.PaymentMethod = PaymentMethod == "—" ? string.Empty : PaymentMethod.Trim();
            receipt.LineItemsJson = LineItems.Count > 0 ? LineItemsHelper.Serialize(LineItems) : string.Empty;

            var merchantChanged = ReceiptId == 0 ||
                                  !string.Equals(Merchant.Trim(), _merchantSnapshot.Trim(), StringComparison.OrdinalIgnoreCase);

            if (merchantChanged || string.IsNullOrWhiteSpace(receipt.MerchantLogoPath))
            {
                receipt.MerchantLogoPath =
                    await _merchantLogoService.TryResolveLogoPathAsync(Merchant.Trim()) ?? string.Empty;
            }

            MerchantLogoPath = receipt.MerchantLogoPath;
            UpdateHeroLogoDisplay();

            await _receiptRepository.SaveAsync(receipt);

            if (ReceiptId > 0)
            {
                IsEditing = false;
                SnapshotFields();
            }

            await NavigateBackAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Edit()
    {
        SnapshotFields();
        IsEditing = true;
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        var descriptionLine = string.IsNullOrWhiteSpace(Description)
            ? string.Empty
            : $"\n{Description.Trim()}";

        var locationLine = string.IsNullOrWhiteSpace(Address)
            ? string.Empty
            : $"\n{Address.Trim()}";

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Receipt",
            Text = $"{Merchant} — {Total} on {ReceiptDate:dd MMM yyyy}{locationLine}{descriptionLine}"
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (IsEditing && ReceiptId > 0)
        {
            RestoreSnapshot();
            IsEditing = false;
            return;
        }

        if (ReceiptId == 0)
        {
            await DiscardNewReceiptAsync();
            return;
        }

        await NavigateBackAsync();
    }

    [RelayCommand]
    private async Task CancelEditAsync()
    {
        if (ReceiptId == 0)
        {
            await DiscardNewReceiptAsync();
            return;
        }

        RestoreSnapshot();
        IsEditing = false;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (ReceiptId <= 0)
        {
            await DiscardNewReceiptAsync();
            return;
        }

        var confirm = await Shell.Current.DisplayAlert(
            "Delete receipt",
            "Are you sure you want to delete this receipt?",
            "Delete",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        var receipt = await _receiptRepository.GetByIdAsync(ReceiptId);
        if (receipt is not null && !string.IsNullOrWhiteSpace(receipt.ImagePath))
        {
            await _imageStorageService.DeleteImageAsync(receipt.ImagePath);
        }

        await _receiptRepository.DeleteAsync(ReceiptId);
        await NavigateBackAsync();
    }

    private async Task DiscardNewReceiptAsync()
    {
        if (!string.IsNullOrWhiteSpace(ImagePath))
        {
            await _imageStorageService.DeleteImageAsync(ImagePath);
        }

        await NavigateBackAsync();
    }

    private async Task NavigateBackAsync()
    {
        var route = ReturnTo switch
        {
            "history" => "//history",
            "dashboard" => AppNavigation.Dashboard,
            "scan" => "//scan",
            _ => ".."
        };

        await Shell.Current.GoToAsync(route);
    }
}
