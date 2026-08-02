using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using TabulateAI.Services;

namespace TabulateAI;

/// <summary>
/// Android local notifications via NotificationManager — visible in the shade even when the app is closed/backgrounded.
/// </summary>
public sealed class LocalNotificationService : ILocalNotificationService
{
    public const string TitleKey = "title";
    public const string MessageKey = "message";

    private const string ChannelId = "budget_alerts";
    private const string ChannelName = "Budget alerts";
    private const string ChannelDescription = "Alerts when a category goes over budget.";

    private static int _messageId;
    private static int _pendingIntentId;

    private bool _channelInitialized;
    private NotificationManagerCompat? _compatManager;

    public static LocalNotificationService? Instance { get; private set; }

    public LocalNotificationService()
    {
        Instance ??= this;
        EnsureChannel();
        _compatManager = NotificationManagerCompat.From(Platform.AppContext);
    }

    public async Task<bool> EnsurePermissionAsync()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return true;
        }

        var status = await Permissions.CheckStatusAsync<NotificationPermission>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        status = await Permissions.RequestAsync<NotificationPermission>();
        return status == PermissionStatus.Granted;
    }

    public async Task ShowAsync(string title, string message, DateTime? notifyTime = null)
    {
        if (!await EnsurePermissionAsync())
        {
            return;
        }

        EnsureChannel();

        if (notifyTime is { } when && when > DateTime.Now.AddSeconds(1))
        {
            Schedule(title, message, when);
            return;
        }

        ShowNow(title, message);
    }

    public void ShowNow(string title, string message)
    {
        EnsureChannel();
        _compatManager ??= NotificationManagerCompat.From(Platform.AppContext);

        var intent = new Intent(Platform.AppContext, typeof(MainActivity));
        intent.PutExtra(TitleKey, title);
        intent.PutExtra(MessageKey, message);
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            : PendingIntentFlags.UpdateCurrent;

        var pendingIntent = PendingIntent.GetActivity(
            Platform.AppContext,
            _pendingIntentId++,
            intent,
            pendingIntentFlags)
            ?? throw new InvalidOperationException("Could not create notification PendingIntent.");

        var builder = new NotificationCompat.Builder(Platform.AppContext, ChannelId)
            .SetContentIntent(pendingIntent)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
            .SetSmallIcon(Resource.Drawable.notification_small)
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh);

        _compatManager!.Notify(_messageId++, builder.Build());
    }

    private void Schedule(string title, string message, DateTime notifyTime)
    {
        var intent = new Intent(Platform.AppContext, typeof(AlarmHandler));
        intent.PutExtra(TitleKey, title);
        intent.PutExtra(MessageKey, message);

        var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
            : PendingIntentFlags.CancelCurrent;

        var pendingIntent = PendingIntent.GetBroadcast(
            Platform.AppContext,
            _pendingIntentId++,
            intent,
            pendingIntentFlags);

        if (pendingIntent is null ||
            Platform.AppContext.GetSystemService(Context.AlarmService) is not AlarmManager alarmManager)
        {
            ShowNow(title, message);
            return;
        }

        var triggerMillis = GetNotifyTimeMillis(notifyTime);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerMillis, pendingIntent);
        }
        else
        {
            alarmManager.Set(AlarmType.RtcWakeup, triggerMillis, pendingIntent);
        }
    }

    private void EnsureChannel()
    {
        if (_channelInitialized || Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            _channelInitialized = true;
            return;
        }

        var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High)
        {
            Description = ChannelDescription
        };

        var manager = (NotificationManager?)Platform.AppContext.GetSystemService(Context.NotificationService);
        manager?.CreateNotificationChannel(channel);
        _channelInitialized = true;
    }

    private static long GetNotifyTimeMillis(DateTime notifyTime)
    {
        var utc = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(utc - epoch).TotalMilliseconds;
    }
}
