using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TabulateAI.Helpers;
using TabulateAI.Models;
using TabulateAI.Services;

namespace TabulateAI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IMerchantLogoService _merchantLogoService;

    [ObservableProperty]
    private List<ReceiptGroup> _groupedReceipts = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _showEmptyState = true;

    [ObservableProperty]
    private string _subtitle = "0 receipts";

    [ObservableProperty]
    private ReceiptDisplayItem? _selectedReceipt;

    public ObservableCollection<CategoryOption> FilterOptions { get; } = [];

    public HistoryViewModel(
        IReceiptRepository receiptRepository,
        IMerchantLogoService merchantLogoService)
    {
        _receiptRepository = receiptRepository;
        _merchantLogoService = merchantLogoService;
        InitializeFilters();
    }

    private void InitializeFilters()
    {
        var filters = new[]
        {
            "All",
            ExpenseCategories.Groceries,
            ExpenseCategories.Food,
            ExpenseCategories.Transport,
            ExpenseCategories.Shopping,
            ExpenseCategories.Health,
            ExpenseCategories.Bills,
            ExpenseCategories.Other
        };

        FilterOptions.Clear();
        foreach (var name in filters)
        {
            FilterOptions.Add(new CategoryOption
            {
                Name = name,
                IsSelected = name == "All"
            });
        }
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

    partial void OnSearchQueryChanged(string value)
    {
        _ = LoadReceiptsAsync();
    }

    partial void OnSelectedReceiptChanged(ReceiptDisplayItem? value)
    {
        if (value is null)
        {
            return;
        }

        SelectedReceipt = null;
        _ = OpenReceiptAsync(value);
    }

    [RelayCommand]
    private void SelectCategory(CategoryOption option)
    {
        SelectedCategory = option.Name;

        foreach (var filter in FilterOptions)
        {
            filter.IsSelected = string.Equals(filter.Name, option.Name, StringComparison.OrdinalIgnoreCase);
        }

        _ = LoadReceiptsAsync();
    }

    [RelayCommand]
    private async Task OpenReceiptAsync(ReceiptDisplayItem item)
    {
        if (item.Id > 0)
        {
            await Shell.Current.GoToAsync($"receiptdetail?ReceiptId={item.Id}&ReturnTo=history");
        }
    }

    [RelayCommand]
    private async Task ScanNowAsync()
    {
        await Shell.Current.GoToAsync("//scan");
    }

    private async Task LoadReceiptsAsync()
    {
        var receipts = string.IsNullOrWhiteSpace(SearchQuery)
            ? await _receiptRepository.GetAllAsync()
            : await _receiptRepository.SearchAsync(SearchQuery);

        if (SelectedCategory != "All")
        {
            receipts = receipts
                .Where(r => MatchesCategoryFilter(r, SelectedCategory))
                .ToList();
        }

        if (await _merchantLogoService.BackfillMissingLogosAsync(receipts, _receiptRepository) > 0)
        {
            receipts = string.IsNullOrWhiteSpace(SearchQuery)
                ? await _receiptRepository.GetAllAsync()
                : await _receiptRepository.SearchAsync(SearchQuery);

            if (SelectedCategory != "All")
            {
                receipts = receipts
                    .Where(r => MatchesCategoryFilter(r, SelectedCategory))
                    .ToList();
            }
        }

        if (receipts.Count == 0)
        {
            GroupedReceipts = [];
            ShowEmptyState = true;
            Subtitle = BuildSubtitle(0);
            return;
        }

        var items = receipts
            .Select(ReceiptDisplayHelper.ToDisplayItem)
            .OrderByDescending(r => r.Date)
            .ToList();

        GroupedReceipts = items
            .GroupBy(r => r.Date.ToString("MMMM yyyy").ToUpperInvariant())
            .Select(BuildGroup)
            .ToList();

        ShowEmptyState = items.Count == 0;
        Subtitle = BuildSubtitle(receipts.Count);
    }

    private static bool MatchesCategoryFilter(Receipt receipt, string filter)
    {
        if (string.Equals(filter, ExpenseCategories.Groceries, StringComparison.OrdinalIgnoreCase))
        {
            return receipt.Category.Equals(ExpenseCategories.Groceries, StringComparison.OrdinalIgnoreCase) ||
                   receipt.Category.Contains("Grocery", StringComparison.OrdinalIgnoreCase);
        }

        return receipt.Category.Equals(filter, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSubtitle(int count)
    {
        var monthYear = DateTime.Now.ToString("MMMM yyyy");
        return count == 0 ? "0 receipts" : $"{count} total · {monthYear}";
    }

    private static ReceiptGroup BuildGroup(IGrouping<string, ReceiptDisplayItem> group)
    {
        var items = group.ToList();
        for (var i = 0; i < items.Count; i++)
        {
            var isFirst = i == 0;
            var isLast = i == items.Count - 1;
            items[i].IsFirstInGroup = isFirst;
            items[i].IsLastInGroup = isLast;
            items[i].ShowDivider = i < items.Count - 1;
            items[i].ItemCornerRadius = new CornerRadius(
                isFirst ? 14 : 0,
                isFirst ? 14 : 0,
                isLast ? 14 : 0,
                isLast ? 14 : 0);
        }

        return new ReceiptGroup(group.Key, items);
    }
}
