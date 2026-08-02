using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace TabulateAI;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationFromIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        CreateNotificationFromIntent(intent);
    }

    static void CreateNotificationFromIntent(Intent? intent)
    {
        if (intent?.Extras is null)
        {
            return;
        }

        // Tapping a local notification relaunches/resumes the app with these extras.
        _ = intent.GetStringExtra(LocalNotificationService.TitleKey);
        _ = intent.GetStringExtra(LocalNotificationService.MessageKey);
    }
}
