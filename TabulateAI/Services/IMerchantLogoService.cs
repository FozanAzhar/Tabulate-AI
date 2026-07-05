using TabulateAI.Models;

namespace TabulateAI.Services;

public interface IMerchantLogoService
{
    Task<string?> TryResolveLogoPathAsync(string merchant, CancellationToken cancellationToken = default);

    Task<int> BackfillMissingLogosAsync(IReadOnlyList<Receipt> receipts, IReceiptRepository repository, int maxCount = 20);
}
