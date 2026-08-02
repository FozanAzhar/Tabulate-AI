namespace TabulateAI.Services;

/// <summary>
/// No-op fallback for platforms without a local-notification implementation.
/// </summary>
public sealed class NullLocalNotificationService : ILocalNotificationService
{
    public Task<bool> EnsurePermissionAsync() => Task.FromResult(false);

    public Task ShowAsync(string title, string message, DateTime? notifyTime = null) =>
        Task.CompletedTask;
}
