namespace TabulateAI.Helpers;

public static class ThemeColors
{
    public static Color Get(string key, string fallbackHex)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
        {
            return color;
        }

        return Color.FromArgb(fallbackHex);
    }

    public static bool IsDarkMode =>
        Application.Current?.UserAppTheme == AppTheme.Dark;
}
