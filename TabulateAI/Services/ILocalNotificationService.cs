namespace TabulateAI.Services;

/// <summary>
/// OS-level local notifications that appear outside the app (status bar / notification shade).
/// </summary>
public interface ILocalNotificationService
{
    Task<bool> EnsurePermissionAsync();

    /// <param name="notifyTime">
    /// When set, schedules the notification for that time so it can appear after leaving the app.
    /// When null, shows immediately.
    /// </param>
    Task ShowAsync(string title, string message, DateTime? notifyTime = null);
}
