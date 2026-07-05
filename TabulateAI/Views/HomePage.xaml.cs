using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        if (Platform.CurrentActivity?.Window is not null)
        {
            Platform.CurrentActivity.Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#09090F"));
        }
#endif
    }
}
