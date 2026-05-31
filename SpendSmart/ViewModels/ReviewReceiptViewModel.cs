using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpendSmart.Models;
using SpendSmart.Services;

namespace SpendSmart.ViewModels;

[QueryProperty(nameof(ReceiptId), "ReceiptId")]
[QueryProperty(nameof(ImagePath), "ImagePath")]
[QueryProperty(nameof(Merchant), "Merchant")]
[QueryProperty(nameof(AmountText), "Amount")]
[QueryProperty(nameof(DateText), "Date")]
[QueryProperty(nameof(Category), "Category")]
[QueryProperty(nameof(RawOcrText), "RawOcrText")]
public partial class ReviewReceiptViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;

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
    private string _rawOcrText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _pageTitle = "Review Receipt";

    [ObservableProperty]
    private ImageSource? _receiptImage;

    public IReadOnlyList<string> Categories { get; } = ExpenseCategories.All;

    public ReviewReceiptViewModel(IReceiptRepository receiptRepository, IImageStorageService imageStorageService)
    {
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
    }

    partial void OnReceiptIdChanged(int value)
    {
        if (value > 0)
        {
            PageTitle = "Edit Receipt";
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
            ReceiptDate = parsed.Date;
        }
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
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Merchant))
        {
            await Shell.Current.DisplayAlertAsync("Missing merchant", "Please enter a merchant name.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Invalid amount", "Please enter a valid amount greater than zero.", "OK");
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
            return;
        }

        var confirm = await Shell.Current.DisplayAlertAsync(
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
        await Shell.Current.GoToAsync("..");
    }
}
