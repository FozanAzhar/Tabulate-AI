using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using TabulateAI.Helpers;

using TabulateAI.Models;

using TabulateAI.Services;



namespace TabulateAI.ViewModels;



public partial class ManualExpenseViewModel : ObservableObject

{

    private readonly IReceiptRepository _receiptRepository;
    private readonly IMerchantLogoService _merchantLogoService;



    [ObservableProperty]

    private string _merchant = string.Empty;



    [ObservableProperty]

    private string _amountText = string.Empty;



    [ObservableProperty]

    private DateTime _expenseDate = DateTime.Today;



    [ObservableProperty]

    private string _selectedCategory = ExpenseCategories.Other;



    [ObservableProperty]

    private string _customCategory = string.Empty;



    [ObservableProperty]

    private string _description = string.Empty;



    [ObservableProperty]

    private bool _isBusy;



    public ObservableCollection<CategoryOption> CategoryOptions { get; } = new();



    public ManualExpenseViewModel(IReceiptRepository receiptRepository, IMerchantLogoService merchantLogoService)

    {

        _receiptRepository = receiptRepository;
        _merchantLogoService = merchantLogoService;

        RefreshCategoryOptions();

    }



    partial void OnMerchantChanged(string value)

    {

        if (string.IsNullOrWhiteSpace(value))

        {

            return;

        }



        var suggested = CategorySuggestionService.SuggestCategory(value);

        SelectCategoryByName(suggested);

    }



    partial void OnCustomCategoryChanged(string value)

    {

        if (!string.IsNullOrWhiteSpace(value))

        {

            SelectedCategory = value.Trim();

            SyncCategorySelection(SelectedCategory);

        }

    }



    [RelayCommand]

    private void SelectCategory(CategoryOption option)

    {

        CustomCategory = string.Empty;

        SelectedCategory = option.Name;

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

            await Shell.Current.DisplayAlert("Missing store", "Please enter where you spent the money.", "OK");

            return;

        }



        if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,

                System.Globalization.CultureInfo.InvariantCulture, out var amount) &&

            !decimal.TryParse(AmountText, out amount))

        {

            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid amount greater than zero.", "OK");

            return;

        }



        if (amount <= 0)

        {

            await Shell.Current.DisplayAlert("Invalid amount", "Please enter a valid amount greater than zero.", "OK");

            return;

        }



        IsBusy = true;



        try

        {

            var category = !string.IsNullOrWhiteSpace(CustomCategory)

                ? CustomCategory.Trim()

                : SelectedCategory;

            CategoryHelper.AddCustomCategory(category);



            var logoPath = await _merchantLogoService.TryResolveLogoPathAsync(Merchant.Trim());



            await _receiptRepository.SaveAsync(new Receipt

            {

                Merchant = Merchant.Trim(),

                Amount = amount,

                Date = ExpenseDate.Date,

                Category = category,

                Description = Description.Trim(),

                ImagePath = string.Empty,

                RawOcrText = string.Empty,

                MerchantLogoPath = logoPath ?? string.Empty

            });



            await AppNavigation.GoDashboardAsync();

        }

        finally

        {

            IsBusy = false;

        }

    }



    [RelayCommand]

    private async Task CancelAsync() => await AppNavigation.GoDashboardAsync();



    private void SelectCategoryByName(string category)

    {

        CustomCategory = string.Empty;

        SelectedCategory = category;

        SyncCategorySelection(category);

    }



    private void RefreshCategoryOptions()

    {

        CategoryOptions.Clear();

        foreach (var name in CategoryHelper.GetAllCategories())

        {

            CategoryOptions.Add(new CategoryOption

            {

                Name = name,

                IsSelected = string.Equals(name, SelectedCategory, StringComparison.OrdinalIgnoreCase)

                    && string.IsNullOrWhiteSpace(CustomCategory)

            });

        }

    }



    private void SyncCategorySelection(string selected)

    {

        foreach (var option in CategoryOptions)

        {

            option.IsSelected = string.Equals(option.Name, selected, StringComparison.OrdinalIgnoreCase)

                && string.IsNullOrWhiteSpace(CustomCategory);

        }

    }

}


