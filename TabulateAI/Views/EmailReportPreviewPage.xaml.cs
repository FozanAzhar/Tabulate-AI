using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class EmailReportPreviewPage : ContentPage
{
    private readonly EmailReportPreviewViewModel _viewModel;

    public EmailReportPreviewPage(EmailReportPreviewViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
