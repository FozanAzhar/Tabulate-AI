namespace TabulateAI.Helpers;

#if ANDROID
using Microsoft.Maui.Platform;
#endif

public static class ThemeResourceHelper
{
    private static readonly (string Key, string Light, string Dark)[] ThemeColorsMap =
    [
        ("Surface", "#F9FAFB", "#09090F"),
        ("CardWhite", "#FFFFFF", "#18181B"),
        ("T1", "#111827", "#FAFAFA"),
        ("T2", "#6B7280", "#A1A1AA"),
        ("T3", "#9CA3AF", "#71717A"),
        ("Border", "#E5E7EB", "#27272A"),
        ("Border2", "#F3F4F6", "#1F1F23"),
        ("VioletTint", "#F5F3FF", "#221633"),
        ("VioletMid", "#EDE9FE", "#2E1F47"),
        ("AppBackground", "#F9FAFB", "#09090F"),
        ("TextPrimary", "#111827", "#FAFAFA"),
        ("TextMuted", "#6B7280", "#A1A1AA"),
        ("BorderFaint", "#E5E7EB", "#27272A"),
    ];

    public static void Apply(bool darkMode)
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
        {
            return;
        }

        foreach (var (key, light, dark) in ThemeColorsMap)
        {
            resources[key] = Color.FromArgb(darkMode ? dark : light);
        }

        ApplyShellColors();
    }

    public static void ApplyShellColors()
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
        {
            return;
        }

        if (Shell.Current is Shell shell)
        {
            shell.SetValue(Shell.BackgroundColorProperty, (Color)resources["Surface"]);
            shell.SetValue(Shell.TabBarBackgroundColorProperty, (Color)resources["CardWhite"]);
            shell.SetValue(Shell.TabBarUnselectedColorProperty, (Color)resources["T3"]);
            shell.SetValue(Shell.ForegroundColorProperty, (Color)resources["T1"]);
            shell.SetValue(Shell.TitleColorProperty, (Color)resources["T1"]);
        }

#if ANDROID
        ApplyAndroidTabBarColors((Color)resources["CardWhite"], (Color)resources["T3"]);
#endif
    }

#if ANDROID
    private static void ApplyAndroidTabBarColors(Color background, Color unselected)
    {
        if (Platform.CurrentActivity?.Window?.DecorView is not Android.Views.ViewGroup root)
        {
            return;
        }

        var bottomNav = FindChild<Google.Android.Material.BottomNavigation.BottomNavigationView>(root);
        bottomNav?.Post(() =>
        {
            bottomNav.SetBackgroundColor(background.ToPlatform());
            bottomNav.ItemIconTintList = CreateTabTintList(
                ThemeColors.Get("Violet", "#7C3AED").ToPlatform(),
                unselected.ToPlatform());
            bottomNav.ItemTextColor = CreateTabTintList(
                ThemeColors.Get("Violet", "#7C3AED").ToPlatform(),
                unselected.ToPlatform());
        });
    }

    private static Android.Content.Res.ColorStateList CreateTabTintList(int selected, int unselected)
    {
        var states = new[]
        {
            new[] { Android.Resource.Attribute.StateChecked },
            new[] { -Android.Resource.Attribute.StateChecked }
        };
        var colors = new[] { selected, unselected };
        return new Android.Content.Res.ColorStateList(states, colors);
    }

    private static T? FindChild<T>(Android.Views.ViewGroup parent) where T : Android.Views.View
    {
        for (var i = 0; i < parent.ChildCount; i++)
        {
            if (parent.GetChildAt(i) is T match)
            {
                return match;
            }

            if (parent.GetChildAt(i) is Android.Views.ViewGroup group)
            {
                var found = FindChild<T>(group);
                if (found is not null)
                {
                    return found;
                }
            }
        }

        return null;
    }
#endif
}
