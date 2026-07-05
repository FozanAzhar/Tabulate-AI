using TabulateAI.Helpers;

namespace TabulateAI.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private const string DisplayNameKey = "settings_display_name";
    private const string EmailKey = "settings_email";
    private const string ThemeKey = "settings_theme";
    private const string CustomReportStartKey = "custom_report_start";
    private const string CustomReportEndKey = "custom_report_end";

    private string _displayName = "User";
    private string _email = string.Empty;
    private bool _isDarkMode;
    private DateTime _customReportStart;
    private DateTime _customReportEnd;

    public event EventHandler? SettingsChanged;

    public string DisplayName
    {
        get => _displayName;
        set
        {
            var trimmed = value?.Trim() ?? string.Empty;
            if (string.Equals(_displayName, trimmed, StringComparison.Ordinal))
            {
                return;
            }

            _displayName = string.IsNullOrWhiteSpace(trimmed) ? "User" : trimmed;
            Save();
            NotifyChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            var trimmed = value?.Trim() ?? string.Empty;
            if (string.Equals(_email, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _email = trimmed;
            Save();
            NotifyChanged();
        }
    }

    public bool IsDarkMode => _isDarkMode;

    public string ThemeIcon => _isDarkMode ? AppIcons.Sun : AppIcons.Moon;

    public string UserInitials => BuildInitials(_displayName);

    public DateTime CustomReportStart
    {
        get => _customReportStart;
        set
        {
            var normalized = value.Date;
            if (_customReportStart == normalized)
            {
                return;
            }

            _customReportStart = normalized;
            Preferences.Default.Set(CustomReportStartKey, normalized.ToString("O"));
            NotifyChanged();
        }
    }

    public DateTime CustomReportEnd
    {
        get => _customReportEnd;
        set
        {
            var normalized = value.Date;
            if (_customReportEnd == normalized)
            {
                return;
            }

            _customReportEnd = normalized;
            Preferences.Default.Set(CustomReportEndKey, normalized.ToString("O"));
            NotifyChanged();
        }
    }

    public void Load()
    {
        _displayName = Preferences.Default.Get(DisplayNameKey, "User");
        if (string.IsNullOrWhiteSpace(_displayName))
        {
            _displayName = "User";
        }

        _email = Preferences.Default.Get(EmailKey, string.Empty);
        _customReportStart = ReadDatePreference(CustomReportStartKey, DateTime.Today.AddDays(-30));
        _customReportEnd = ReadDatePreference(CustomReportEndKey, DateTime.Today);
        ApplySavedTheme();
    }

    private static DateTime ReadDatePreference(string key, DateTime fallback)
    {
        var stored = Preferences.Default.Get(key, string.Empty);
        return DateTime.TryParse(stored, out var parsed) ? parsed.Date : fallback.Date;
    }

    public void Save()
    {
        Preferences.Default.Set(DisplayNameKey, _displayName);
        Preferences.Default.Set(EmailKey, _email);
    }

    public void ToggleTheme()
    {
        SetTheme(!_isDarkMode);
    }

    public void SetDarkMode(bool enabled)
    {
        SetTheme(enabled);
    }

    public void ApplySavedTheme()
    {
        var saved = Preferences.Default.Get(ThemeKey, AppTheme.Light.ToString());
        var theme = Enum.TryParse<AppTheme>(saved, out var parsed) ? parsed : AppTheme.Light;
        SetTheme(theme == AppTheme.Dark, persist: false);
        Preferences.Default.Set(ThemeKey, theme.ToString());
    }

    private void SetTheme(bool darkMode, bool persist = true)
    {
        if (_isDarkMode == darkMode && Application.Current?.UserAppTheme == (darkMode ? AppTheme.Dark : AppTheme.Light))
        {
            return;
        }

        _isDarkMode = darkMode;
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = darkMode ? AppTheme.Dark : AppTheme.Light;
            ThemeResourceHelper.Apply(darkMode);
        }

        if (persist)
        {
            Preferences.Default.Set(ThemeKey, darkMode ? AppTheme.Dark.ToString() : AppTheme.Light.ToString());
        }

        NotifyChanged();
    }

    private void NotifyChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);

    private static string BuildInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "U";
        }

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0][..2].ToUpperInvariant()
                : parts[0][0].ToString().ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
