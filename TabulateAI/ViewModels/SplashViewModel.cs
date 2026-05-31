using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TabulateAI.ViewModels;

public partial class SplashViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GetStartedAsync()
    {
        if (Application.Current?.Windows.FirstOrDefault() is not Window window)
        {
            return;
        }

        window.Page = new AppShell();
        await Shell.Current.GoToAsync("//DashboardPage");
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return;
        }

        await page.DisplayAlert(
            "UoN Account",
            "Sign in with your University of Newcastle account would connect here.",
            "OK");
    }
}
