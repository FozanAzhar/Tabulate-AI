using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ReviewReceiptPage : ContentPage
{
    public ReviewReceiptPage(ReviewReceiptViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReviewReceiptViewModel viewModel)
        {
            viewModel.FinalizeLoadedState();
        }
    }
}
