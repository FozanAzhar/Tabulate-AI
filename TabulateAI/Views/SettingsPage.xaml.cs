using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class SettingsPage : ContentPage
{
    private bool _suppressThemeSwitchEvent;
    private bool _suppressBudgetAlertsSwitchEvent;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (BindingContext is not SettingsViewModel viewModel)
            {
                return;
            }

            await viewModel.InitializeAsync();

            _suppressThemeSwitchEvent = true;
            if (DarkModeSwitch is not null)
            {
                DarkModeSwitch.IsToggled = viewModel.IsDarkMode;
            }
            _suppressThemeSwitchEvent = false;

            _suppressBudgetAlertsSwitchEvent = true;
            if (BudgetAlertsSwitch is not null)
            {
                BudgetAlertsSwitch.IsToggled = viewModel.BudgetAlertsEnabled;
            }
            _suppressBudgetAlertsSwitchEvent = false;
        }
        catch (Exception ex)
        {
            App.WriteCrashLog($"SettingsPage.OnAppearing: {ex}");
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

    private async void OnBudgetAlertsToggled(object? sender, ToggledEventArgs e)
    {
        if (_suppressBudgetAlertsSwitchEvent || BindingContext is not SettingsViewModel viewModel)
        {
            return;
        }

        try
        {
            await viewModel.SetBudgetAlertsEnabledAsync(e.Value);
        }
        catch (Exception ex)
        {
            App.WriteCrashLog($"SettingsPage.OnBudgetAlertsToggled: {ex}");
        }

        _suppressBudgetAlertsSwitchEvent = true;
        if (BudgetAlertsSwitch is not null)
        {
            BudgetAlertsSwitch.IsToggled = viewModel.BudgetAlertsEnabled;
        }
        _suppressBudgetAlertsSwitchEvent = false;
    }
}
