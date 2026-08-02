namespace TabulateAI.Models;

public sealed class AppNotificationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsRead { get; set; }

    public string TimeLabel =>
        CreatedAt.Date == DateTime.Today
            ? CreatedAt.ToString("h:mm tt")
            : CreatedAt.ToString("dd MMM · h:mm tt");
}
