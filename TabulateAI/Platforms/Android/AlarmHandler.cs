using Android.Content;

namespace TabulateAI;

[BroadcastReceiver(Enabled = true, Exported = false, Label = "Local Notifications Broadcast Receiver")]
public sealed class AlarmHandler : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Extras is null)
        {
            return;
        }

        var title = intent.GetStringExtra(LocalNotificationService.TitleKey) ?? "Expensely";
        var message = intent.GetStringExtra(LocalNotificationService.MessageKey) ?? string.Empty;

        var manager = LocalNotificationService.Instance ?? new LocalNotificationService();
        manager.ShowNow(title, message);
    }
}
