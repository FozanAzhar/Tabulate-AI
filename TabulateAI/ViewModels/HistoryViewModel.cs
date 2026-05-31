using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private List<ReceiptGroup> _groupedReceipts = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "All";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _showEmptyState;

    [ObservableProperty]
    private string _subtitle = "34 total this month";

    public IReadOnlyList<string> Filters { get; } =
        ["All", "Grocery", "Travel", "Food", "Health", "Office"];

    public HistoryViewModel(IReceiptRepository receiptRepository, IImageStorageService imageStorageService)
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
    private void SelectFilter(string filter)
    {
        SelectedFilter = filter;
        _ = LoadReceiptsAsync();
    }

    [RelayCommand]
    private async Task OpenReceiptAsync(ReceiptDisplayItem item)
    {
        if (item.Id > 0)
        {
            await Shell.Current.GoToAsync($"ReviewReceipt?ReceiptId={item.Id}");
        }
    }

    [RelayCommand]
    private async Task ScanNowAsync()
    {
        await Shell.Current.GoToAsync("//ScanPage");
    }

    [RelayCommand]
    private async Task ShowFiltersAsync()
    {
        await Shell.Current.DisplayAlert("Filters", "Filter options would appear here.", "OK");
    }

    private async Task LoadReceiptsAsync()
    {
        var receipts = string.IsNullOrWhiteSpace(SearchText)
            ? await _receiptRepository.GetAllAsync()
            : await _receiptRepository.SearchAsync(SearchText);

        if (receipts.Count == 0)
        {
            ApplySampleData();
            return;
        }

        if (SelectedFilter != "All")
        {
            receipts = receipts
                .Where(r => r.Category.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var items = receipts.Select(ReceiptDisplayHelper.ToDisplayItem).ToList();
        GroupedReceipts = items
            .GroupBy(r => r.Date.ToString("MMMM yyyy").ToUpperInvariant())
            .Select(g => new ReceiptGroup(g.Key, g.OrderByDescending(r => r.Date)))
            .ToList();

        ShowEmptyState = items.Count == 0;
        Subtitle = $"{receipts.Count} total this month";
    }

    private void ApplySampleData()
    {
        var samples = ReceiptDisplayHelper.GetSampleReceipts();
        samples.AddRange(
        [
            new ReceiptDisplayItem
            {
                Name = "Chemist Warehouse",
                Meta = "Health · 9 May",
                Amount = 24.99m,
                Icon = IconGlyphs.FirstAid,
                IconBackground = Color.FromArgb("#F0F4F8"),
                IconColor = Color.FromArgb("#003058"),
                Category = "Health",
                Date = new DateTime(2026, 5, 9)
            },
            new ReceiptDisplayItem
            {
                Name = "Sydney Airport",
                Meta = "Travel · 8 May",
                Amount = 18.00m,
                Icon = IconGlyphs.Plane,
                IconBackground = Color.FromArgb("#FFF8EC"),
                IconColor = Color.FromArgb("#C8922A"),
                Category = "Travel",
                Date = new DateTime(2026, 5, 8)
            },
            new ReceiptDisplayItem
            {
                Name = "JB Hi-Fi",
                Meta = "Office · 7 May",
                Amount = 129.00m,
                Icon = IconGlyphs.Laptop,
                IconBackground = Color.FromArgb("#E8F2F9"),
                IconColor = Color.FromArgb("#003058"),
                Category = "Office",
                Date = new DateTime(2026, 5, 7)
            }
        ]);

        GroupedReceipts = [new ReceiptGroup("MAY 2026", samples)];
        ShowEmptyState = false;
        Subtitle = "34 total this month";
    }
}
