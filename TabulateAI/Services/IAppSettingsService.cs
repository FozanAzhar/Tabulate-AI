namespace TabulateAI.Services;

public interface IAppSettingsService
{
    string DisplayName { get; set; }

    string Email { get; set; }

    bool IsDarkMode { get; }

    string ThemeIcon { get; }

    string UserInitials { get; }

    DateTime CustomReportStart { get; set; }

    DateTime CustomReportEnd { get; set; }

    bool BudgetAlertsEnabled { get; set; }

    event EventHandler? SettingsChanged;

    void Load();

    void Save();

    void ToggleTheme();

    void SetDarkMode(bool enabled);

    void ApplySavedTheme();
}
