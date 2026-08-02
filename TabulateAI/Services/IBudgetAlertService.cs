namespace TabulateAI.Services;

public interface IBudgetAlertService
{
    /// <param name="showInAppAlert">
    /// When true, also shows an in-app dialog for newly detected over-budget categories.
    /// </param>
    Task CheckAndNotifyAsync(bool showInAppAlert = false);
}
