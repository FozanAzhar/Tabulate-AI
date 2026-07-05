using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ManualExpensePage : ContentPage
{
    public ManualExpensePage(ManualExpenseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
