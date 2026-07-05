using TabulateAI.Models;

namespace TabulateAI.Services;

public interface IExpenseExportService
{
    string BuildCsv(IReadOnlyList<Receipt> receipts, string periodLabel);

    Task<string> SaveCsvAsync(string csvContent, string fileToken);

    Task ShareCsvAsync(string filePath);

    Task<string> SavePdfAsync(IReadOnlyList<Receipt> receipts, string periodLabel, string fileToken);

    Task SharePdfAsync(string filePath);

    Task SendEmailReportAsync(string recipient, string subject, string body, string pdfFilePath);
}
