using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpendSmart.Models;
using SpendSmart.Services;

namespace SpendSmart.ViewModels;

public partial class ArchiveViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private List<Receipt> _receipts = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isEmpty;

    public ArchiveViewModel(IReceiptRepository receiptRepository, IImageStorageService imageStorageService)
    {
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task InitializeAsync()
    {
        await LoadReceiptsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;

        try
        {
            await LoadReceiptsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadReceiptsAsync();
    }

    [RelayCommand]
    private async Task OpenReceiptAsync(Receipt receipt)
    {
        await Shell.Current.GoToAsync($"ReviewReceipt?ReceiptId={receipt.Id}");
    }

    [RelayCommand]
    private async Task DeleteReceiptAsync(Receipt receipt)
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete receipt",
            $"Delete receipt from {receipt.Merchant}?",
            "Delete",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(receipt.ImagePath))
        {
            await _imageStorageService.DeleteImageAsync(receipt.ImagePath);
        }

        await _receiptRepository.DeleteAsync(receipt.Id);
        await LoadReceiptsAsync();
    }

    private async Task LoadReceiptsAsync()
    {
        Receipts = string.IsNullOrWhiteSpace(SearchText)
            ? await _receiptRepository.GetAllAsync()
            : await _receiptRepository.SearchAsync(SearchText);

        IsEmpty = Receipts.Count == 0;
    }
}
