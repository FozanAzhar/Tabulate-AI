using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ReviewReceiptPage : ContentPage
{
    public ReviewReceiptPage(ReviewReceiptViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
