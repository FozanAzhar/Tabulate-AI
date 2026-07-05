using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TabulateAI.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GetStartedAsync()
    {
        await Shell.Current.GoToAsync("//dashboard");
    }
}
