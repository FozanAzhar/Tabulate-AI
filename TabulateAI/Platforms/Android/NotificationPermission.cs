using Android;

namespace TabulateAI;

/// <summary>
/// Android 13+ requires runtime POST_NOTIFICATIONS before local notifications can appear.
/// </summary>
public sealed class NotificationPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return [(Manifest.Permission.PostNotifications, true)];
            }

            return [];
        }
    }
}
