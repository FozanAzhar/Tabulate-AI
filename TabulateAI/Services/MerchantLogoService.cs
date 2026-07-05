using TabulateAI.Helpers;
using TabulateAI.Models;

namespace TabulateAI.Services;

public class MerchantLogoService : IMerchantLogoService
{
    private static readonly SemaphoreSlim DownloadLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly string _logoDirectory;

    public MerchantLogoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _logoDirectory = Path.Combine(FileSystem.CacheDirectory, "merchant-logos");
        Directory.CreateDirectory(_logoDirectory);
    }

    public async Task<string?> TryResolveLogoPathAsync(string merchant, CancellationToken cancellationToken = default)
    {
        var brand = MerchantLogoRegistry.Match(merchant);
        if (brand is null)
        {
            return null;
        }

        var cachePath = Path.Combine(_logoDirectory, $"{brand.BrandKey}.png");
        if (File.Exists(cachePath))
        {
            return cachePath;
        }

        await DownloadLock.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(cachePath))
            {
                return cachePath;
            }

            var bytes = await DownloadLogoAsync(brand.Domain, cancellationToken);
            if (bytes is null || bytes.Length < 64)
            {
                return null;
            }

            await File.WriteAllBytesAsync(cachePath, bytes, cancellationToken);
            return cachePath;
        }
        finally
        {
            DownloadLock.Release();
        }
    }

    public async Task<int> BackfillMissingLogosAsync(
        IReadOnlyList<Receipt> receipts,
        IReceiptRepository repository,
        int maxCount = 20)
    {
        var updated = 0;

        foreach (var receipt in receipts
                     .Where(r => string.IsNullOrWhiteSpace(r.MerchantLogoPath))
                     .Take(maxCount))
        {
            var logoPath = await TryResolveLogoPathAsync(receipt.Merchant);
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                continue;
            }

            receipt.MerchantLogoPath = logoPath;
            await repository.SaveAsync(receipt);
            updated++;
        }

        return updated;
    }

    private async Task<byte[]?> DownloadLogoAsync(string domain, CancellationToken cancellationToken)
    {
        var urls = new[]
        {
            $"https://logo.clearbit.com/{domain}",
            $"https://www.google.com/s2/favicons?domain={domain}&sz=128"
        };

        foreach (var url in urls)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length < 64)
                {
                    continue;
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(contentType) &&
                    !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return bytes;
            }
            catch
            {
                // Try the next provider.
            }
        }

        return null;
    }
}
