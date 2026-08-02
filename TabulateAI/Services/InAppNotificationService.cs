using System.Text.Json;
using TabulateAI.Helpers;
using TabulateAI.Models;

namespace TabulateAI.Services;

public sealed class InAppNotificationService : IInAppNotificationService
{
    private const string PreferencesKey = "in_app_notifications";
    private const int MaxStored = 30;

    private readonly ILocalNotificationService _localNotifications;
    private readonly List<AppNotificationItem> _items = [];
    private readonly object _gate = new();

    public event EventHandler? NotificationsChanged;

    public int UnreadCount
    {
        get
        {
            lock (_gate)
            {
                return _items.Count(i => !i.IsRead);
            }
        }
    }

    public InAppNotificationService(ILocalNotificationService localNotifications)
    {
        _localNotifications = localNotifications;
        Load();
    }

    public IReadOnlyList<AppNotificationItem> GetRecent(int max = 20)
    {
        lock (_gate)
        {
            return _items.Take(Math.Max(1, max)).ToList();
        }
    }

    public async Task NotifyAsync(
        string title,
        string message,
        bool showBanner = true,
        TimeSpan? scheduleLocalIn = null)
    {
        var item = new AppNotificationItem
        {
            Title = title.Trim(),
            Message = message.Trim(),
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        lock (_gate)
        {
            _items.Insert(0, item);
            while (_items.Count > MaxStored)
            {
                _items.RemoveAt(_items.Count - 1);
            }

            SaveUnlocked();
        }

        NotificationsChanged?.Invoke(this, EventArgs.Empty);

        // OS local notification — appears in the shade even when the app is backgrounded.
        try
        {
            DateTime? when = scheduleLocalIn is { } delay && delay > TimeSpan.Zero
                ? DateTime.Now.Add(delay)
                : null;
            await _localNotifications.ShowAsync(item.Title, item.Message, when);
        }
        catch (Exception ex)
        {
            App.WriteCrashLog($"InAppNotificationService.LocalNotify: {ex}");
        }

        if (showBanner)
        {
            await InAppBannerHelper.ShowAsync(item.Title, item.Message);
        }
    }

    public void MarkAllRead()
    {
        lock (_gate)
        {
            var changed = false;
            foreach (var item in _items)
            {
                if (item.IsRead)
                {
                    continue;
                }

                item.IsRead = true;
                changed = true;
            }

            if (changed)
            {
                SaveUnlocked();
            }
        }

        // Do not raise NotificationsChanged here — Settings.RefreshAlerts listens for that
        // event and also calls MarkAllRead, which previously caused a StackOverflow crash.
    }

    public void Clear()
    {
        lock (_gate)
        {
            _items.Clear();
            Preferences.Default.Remove(PreferencesKey);
        }

        NotificationsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Load()
    {
        try
        {
            var json = Preferences.Default.Get(PreferencesKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var items = JsonSerializer.Deserialize<List<AppNotificationItem>>(json);
            if (items is null)
            {
                return;
            }

            lock (_gate)
            {
                _items.Clear();
                _items.AddRange(items.OrderByDescending(i => i.CreatedAt).Take(MaxStored));
            }
        }
        catch (Exception ex)
        {
            App.WriteCrashLog($"InAppNotificationService.Load: {ex}");
        }
    }

    private void SaveUnlocked()
    {
        try
        {
            var json = JsonSerializer.Serialize(_items);
            Preferences.Default.Set(PreferencesKey, json);
        }
        catch (Exception ex)
        {
            App.WriteCrashLog($"InAppNotificationService.Save: {ex}");
        }
    }
}
