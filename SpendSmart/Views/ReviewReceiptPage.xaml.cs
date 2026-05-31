using SpendSmart.ViewModels;

namespace SpendSmart.Views;

public partial class ReviewReceiptPage : ContentPage
{
    public ReviewReceiptPage(ReviewReceiptViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
