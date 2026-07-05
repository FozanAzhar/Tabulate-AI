using TabulateAI.Models;

namespace TabulateAI.Services;

public interface IExpenseExportService
{
    string BuildCsv(IReadOnlyList<Receipt> receipts, string periodLabel);

    Task<string> SaveCsvAsync(string csvContent, string fileToken);

    Task ShareCsvAsync(string filePath);
}
