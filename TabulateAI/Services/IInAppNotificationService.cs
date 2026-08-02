using TabulateAI.Models;

namespace TabulateAI.Services;

public interface IInAppNotificationService
{
    event EventHandler? NotificationsChanged;

    IReadOnlyList<AppNotificationItem> GetRecent(int max = 20);

    int UnreadCount { get; }

    /// <param name="showBanner">Show the in-app banner overlay.</param>
    /// <param name="scheduleLocalIn">
    /// When set, also schedules an OS local notification after this delay
    /// (useful for demos: leave the app and watch the shade).
    /// </param>
    Task NotifyAsync(string title, string message, bool showBanner = true, TimeSpan? scheduleLocalIn = null);

    void MarkAllRead();

    void Clear();
}
