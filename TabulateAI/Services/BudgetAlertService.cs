using System.Globalization;
using TabulateAI.Helpers;

namespace TabulateAI.Services;

public sealed class BudgetAlertService : IBudgetAlertService
{
    private const string NotifiedKeyPrefix = "budget_alert_notified_";

    private readonly IReceiptRepository _receiptRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly IInAppNotificationService _notifications;

    public BudgetAlertService(
        IReceiptRepository receiptRepository,
        IAppSettingsService appSettings,
        IInAppNotificationService notifications)
    {
        _receiptRepository = receiptRepository;
        _appSettings = appSettings;
        _notifications = notifications;
    }

    public async Task CheckAndNotifyAsync(bool showInAppAlert = false)
    {
        if (!_appSettings.BudgetAlertsEnabled)
        {
            return;
        }

        var now = DateTime.Now;
        var summaries = await _receiptRepository.GetCategorySummariesAsync(now.Year, now.Month);
        var statuses = BudgetHelper.BuildStatus(summaries);

        foreach (var status in statuses.Where(s => !s.IsOverBudget))
        {
            Preferences.Default.Remove(BuildNotifiedKey(status.Category, now.Year, now.Month));
        }

        foreach (var status in statuses.Where(s => s.IsOverBudget))
        {
            var key = BuildNotifiedKey(status.Category, now.Year, now.Month);
            if (Preferences.Default.Get(key, false))
            {
                continue;
            }

            var overBy = status.Spent - status.Budget;
            var title = $"{status.Category} budget exceeded";
            var message =
                $"You've spent {status.Spent.ToString("C0", CultureInfo.CurrentCulture)} of " +
                $"{status.Budget.ToString("C0", CultureInfo.CurrentCulture)} this month " +
                $"({overBy.ToString("C0", CultureInfo.CurrentCulture)} over).";

            // Show a banner for explicit user actions; dashboard checks still store the alert quietly.
            await _notifications.NotifyAsync(title, message, showBanner: showInAppAlert);
            Preferences.Default.Set(key, true);
        }
    }

    private static string BuildNotifiedKey(string category, int year, int month) =>
        $"{NotifiedKeyPrefix}{year}_{month}_{category.Trim().ToLowerInvariant()}";
}
