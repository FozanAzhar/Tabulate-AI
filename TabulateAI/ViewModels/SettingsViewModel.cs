using CommunityToolkit.Mvvm.ComponentModel;

namespace TabulateAI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = "FA";

    [ObservableProperty]
    private string _userEmail = "c3493921@uon.edu.au";
}
