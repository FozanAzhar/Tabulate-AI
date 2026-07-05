using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ExportPreviewPage : ContentPage
{
    private readonly ExportPreviewViewModel _viewModel;

    public ExportPreviewPage(ExportPreviewViewModel viewModel)
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
