using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class SettingsPage : ContentPage
{
    private bool _suppressThemeSwitchEvent;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SettingsViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            _suppressThemeSwitchEvent = true;
            DarkModeSwitch.IsToggled = viewModel.IsDarkMode;
            _suppressThemeSwitchEvent = false;
        }
    }

    private void OnDarkModeToggled(object? sender, ToggledEventArgs e)
    {
        if (_suppressThemeSwitchEvent || BindingContext is not SettingsViewModel viewModel)
        {
            return;
        }

        viewModel.SetDarkMode(e.Value);
    }
}
