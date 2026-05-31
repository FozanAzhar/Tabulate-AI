using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

[QueryProperty(nameof(ReceiptId), "ReceiptId")]
[QueryProperty(nameof(ImagePath), "ImagePath")]
[QueryProperty(nameof(Merchant), "Merchant")]
[QueryProperty(nameof(AmountText), "Amount")]
[QueryProperty(nameof(DateText), "Date")]
[QueryProperty(nameof(Category), "Category")]
[QueryProperty(nameof(RawOcrText), "RawOcrText")]
[QueryProperty(nameof(ExtractionSource), "ExtractionSource")]
[QueryProperty(nameof(ConfidenceText), "Confidence")]
[QueryProperty(nameof(ValidationIssuesText), "ValidationIssues")]
public partial class ReviewReceiptViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private int _receiptId;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _merchant = "Woolworths";

    [ObservableProperty]
    private string _amountText = "84.60";

    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private DateTime _receiptDate = new(2026, 5, 11, 14, 34, 0);

    [ObservableProperty]
    private string _category = "Grocery";

    [ObservableProperty]
    private string _address = "📍 120 George St, Sydney NSW 2000";

    [ObservableProperty]
    private string _paymentMethod = "Visa ···· 4821";

    [ObservableProperty]
    private string _uploadDestination = "Supabase + imgBB";

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
    private bool _showWarningBanner;

    [ObservableProperty]
    private bool _showDangerBanner;

    [ObservableProperty]
    private string _warningMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ImageSource? _receiptImage;

    [ObservableProperty]
    private List<LineItem> _lineItems = [];

    public string DateChip => ReceiptDate.ToString("dd MMM yyyy");
    public string TimeChip => ReceiptDate.ToString("h:mm tt");
    public string CategoryChip => $"🏷 {Category}";
    public string TotalFormatted => decimal.TryParse(AmountText, out var amt) ? amt.ToString("C2") : AmountText;

    public IReadOnlyList<string> Categories { get; } = ExpenseCategories.All;

    public ReviewReceiptViewModel(IReceiptRepository receiptRepository, IImageStorageService imageStorageService)
    {
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
        LoadSampleLineItems();
    }

    partial void OnConfidenceTextChanged(string value)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var confidence))
        {
            if (confidence <= 1)
            {
                ConfidenceValue = confidence * 100;
                ConfidenceText = confidence.ToString("P0");
            }
            else
            {
                ConfidenceValue = confidence;
            }
        }

        UpdateConfidenceBanner();
    }

    partial void OnReceiptIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadReceiptAsync(value);
        }
    }

    partial void OnMerchantChanged(string value)
    {
        if (ReceiptId == 0 && !string.IsNullOrWhiteSpace(value))
        {
            Category = CategorySuggestionService.SuggestCategory(value);
        }
    }

    partial void OnImagePathChanged(string value)
    {
        UpdateImagePreview(value);
    }

    partial void OnDateTextChanged(string value)
    {
        if (DateTime.TryParse(value, out var parsed))
        {
            ReceiptDate = parsed;
        }
    }

    partial void OnCategoryChanged(string value)
    {
        OnPropertyChanged(nameof(CategoryChip));
    }

    partial void OnReceiptDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(DateChip));
        OnPropertyChanged(nameof(TimeChip));
    }

    partial void OnAmountTextChanged(string value)
    {
        OnPropertyChanged(nameof(TotalFormatted));
    }

    private void UpdateConfidenceBanner()
    {
        ShowWarningBanner = false;
        ShowDangerBanner = false;

        if (ConfidenceValue >= 85 || ConfidenceValue <= 0)
            return;

        if (ConfidenceValue >= 60)
        {
            ShowWarningBanner = true;
            WarningMessage = $"⚠ Some fields may need review — confidence: {(int)ConfidenceValue}%";
        }
        else
        {
            ShowDangerBanner = true;
            WarningMessage = "⚠ Low confidence — please verify all fields";
        }
    }

    private void LoadSampleLineItems()
    {
        LineItems =
        [
            new LineItem { Name = "Bananas 1kg", Price = 3.50m },
            new LineItem { Name = "Full Cream Milk 2L", Price = 3.20m },
            new LineItem { Name = "Sourdough Loaf", Price = 4.80m },
            new LineItem { Name = "Free Range Eggs 12pk", Price = 7.90m },
            new LineItem { Name = "Chicken Breast 500g", Price = 9.50m },
            new LineItem { Name = "Greek Yoghurt 1kg", Price = 6.80m },
            new LineItem { Name = "Staff Discount", Price = -4.20m, IsDiscount = true }
        ];
    }

    private async Task LoadReceiptAsync(int id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt is null)
        {
            return;
        }

        Merchant = receipt.Merchant;
        AmountText = receipt.Amount.ToString("0.00");
        ReceiptDate = receipt.Date;
        Category = receipt.Category;
        ImagePath = receipt.ImagePath;
        RawOcrText = receipt.RawOcrText;
    }

    private void UpdateImagePreview(string path)
    {
        ReceiptImage = !string.IsNullOrWhiteSpace(path) && File.Exists(path)
            ? ImageSource.FromFile(path)
            : null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Merchant))
        {
            await Shell.Current.DisplayAlert("Missing merchant", "Please enter a merchant name.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0)
        {
            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid amount greater than zero.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            var receipt = ReceiptId > 0
                ? await _receiptRepository.GetByIdAsync(ReceiptId) ?? new Receipt()
                : new Receipt();

            receipt.Merchant = Merchant.Trim();
            receipt.Amount = amount;
            receipt.Date = ReceiptDate.Date;
            receipt.Category = Category;
            receipt.ImagePath = ImagePath;
            receipt.RawOcrText = RawOcrText;

            await _receiptRepository.SaveAsync(receipt);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        await Shell.Current.DisplayAlert("Edit", "Edit mode would open here.", "OK");
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Receipt",
            Text = $"{Merchant} — {TotalFormatted} on {DateChip}"
        });
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (ReceiptId == 0 && !string.IsNullOrWhiteSpace(ImagePath))
        {
            await _imageStorageService.DeleteImageAsync(ImagePath);
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (ReceiptId <= 0)
        {
            await CancelAsync();
            return;
        }

        var confirm = await Shell.Current.DisplayAlert(
            "Delete receipt",
            "Are you sure you want to delete this receipt?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        var receipt = await _receiptRepository.GetByIdAsync(ReceiptId);
        if (receipt is not null && !string.IsNullOrWhiteSpace(receipt.ImagePath))
        {
            await _imageStorageService.DeleteImageAsync(receipt.ImagePath);
        }

        await _receiptRepository.DeleteAsync(ReceiptId);
        await Shell.Current.GoToAsync("..");
    }
}
