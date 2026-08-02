using System.IO.Compression;
using System.Text.Json;

namespace TabulateAI.Services;

public sealed class BackupService : IBackupService
{
    private static readonly string[] PreferenceKeys =
    [
        "settings_display_name",
        "settings_email",
        "settings_theme",
        "custom_report_start",
        "custom_report_end",
        "custom_expense_categories",
        "category_budgets",
        "settings_budget_alerts",
        "in_app_notifications"
    ];

    private readonly IReceiptRepository _receiptRepository;
    private readonly IImageStorageService _imageStorageService;

    public BackupService(IReceiptRepository receiptRepository, IImageStorageService imageStorageService)
    {
        _receiptRepository = receiptRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task<string> CreateBackupAsync()
    {
        var backupDir = Path.Combine(FileSystem.CacheDirectory, "backups");
        Directory.CreateDirectory(backupDir);

        var zipPath = Path.Combine(backupDir, $"expensely-backup-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tabulate.db3");
        var receiptsDir = Path.Combine(FileSystem.AppDataDirectory, "receipts");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            if (File.Exists(dbPath))
            {
                zip.CreateEntryFromFile(dbPath, "tabulate.db3", CompressionLevel.Optimal);
            }

            if (Directory.Exists(receiptsDir))
            {
                foreach (var file in Directory.GetFiles(receiptsDir))
                {
                    zip.CreateEntryFromFile(file, $"receipts/{Path.GetFileName(file)}", CompressionLevel.Optimal);
                }
            }

            var settings = PreferenceKeys.ToDictionary(
                key => key,
                key => Preferences.Default.Get(key, string.Empty));

            var settingsJson = JsonSerializer.Serialize(settings);
            var entry = zip.CreateEntry("settings.json");
            await using var writer = new StreamWriter(entry.Open());
            await writer.WriteAsync(settingsJson);
        }

        return zipPath;
    }

    public async Task RestoreBackupAsync(string zipPath)
    {
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException("Backup file not found.", zipPath);
        }

        var extractDir = Path.Combine(FileSystem.CacheDirectory, $"restore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractDir);

        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractDir);

            var dbSource = Path.Combine(extractDir, "tabulate.db3");
            var dbTarget = Path.Combine(FileSystem.AppDataDirectory, "tabulate.db3");
            var receiptsTarget = Path.Combine(FileSystem.AppDataDirectory, "receipts");
            var receiptsSource = Path.Combine(extractDir, "receipts");

            await _receiptRepository.ResetConnectionAsync();
            await _imageStorageService.ClearAllReceiptImagesAsync();

            if (File.Exists(dbTarget))
            {
                File.Delete(dbTarget);
            }

            if (File.Exists(dbSource))
            {
                File.Copy(dbSource, dbTarget, overwrite: true);
            }

            if (Directory.Exists(receiptsSource))
            {
                Directory.CreateDirectory(receiptsTarget);
                foreach (var file in Directory.GetFiles(receiptsSource))
                {
                    File.Copy(file, Path.Combine(receiptsTarget, Path.GetFileName(file)), overwrite: true);
                }
            }

            var settingsPath = Path.Combine(extractDir, "settings.json");
            if (File.Exists(settingsPath))
            {
                var json = await File.ReadAllTextAsync(settingsPath);
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (settings is not null)
                {
                    foreach (var (key, value) in settings)
                    {
                        Preferences.Default.Set(key, value);
                    }
                }
            }

            await _receiptRepository.InitializeAsync();
        }
        finally
        {
            if (Directory.Exists(extractDir))
            {
                Directory.Delete(extractDir, recursive: true);
            }
        }
    }

    public async Task ShareBackupAsync(string zipPath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Expensely backup",
            File = new ShareFile(zipPath)
        });
    }
}
