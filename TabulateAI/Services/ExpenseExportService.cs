using System.Globalization;
using System.Text;
using TabulateAI.Models;

namespace TabulateAI.Services;

public class ExpenseExportService : IExpenseExportService
{
    public string BuildCsv(IReadOnlyList<Receipt> receipts, string periodLabel)
    {
        var builder = new StringBuilder();
        var total = receipts.Sum(r => r.Amount);
        var generated = DateTime.Now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

        AppendRow(builder, "Expensely Expense Report");
        AppendRow(builder, "Period", periodLabel);
        AppendRow(builder, "Generated", generated);
        AppendRow(builder, "Total Receipts", receipts.Count.ToString(CultureInfo.InvariantCulture));
        AppendRow(builder, "Total Amount (AUD)", total.ToString("F2", CultureInfo.InvariantCulture));
        builder.AppendLine();

        AppendRow(builder, "Date", "Merchant", "Category", "Amount (AUD)");
        foreach (var receipt in receipts.OrderByDescending(r => r.Date))
        {
            AppendRow(
                builder,
                receipt.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                receipt.Merchant,
                receipt.Category,
                receipt.Amount.ToString("F2", CultureInfo.InvariantCulture));
        }

        builder.AppendLine();
        AppendRow(builder, "Category Summary");
        AppendRow(builder, "Category", "Count", "Total (AUD)");

        foreach (var group in receipts.GroupBy(r => r.Category).OrderByDescending(g => g.Sum(r => r.Amount)))
        {
            AppendRow(
                builder,
                group.Key,
                group.Count().ToString(CultureInfo.InvariantCulture),
                group.Sum(r => r.Amount).ToString("F2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    public async Task<string> SaveCsvAsync(string csvContent, string fileToken)
    {
        var exportDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Expensely",
            "Exports");

        Directory.CreateDirectory(exportDirectory);

        var fileName = $"Expensely_Report_{fileToken}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(exportDirectory, fileName);

        await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
        return filePath;
    }

    public async Task OpenCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        await Launcher.Default.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(filePath, "text/csv")
        });
    }

    private static void AppendRow(StringBuilder builder, params string?[] values)
    {
        builder.AppendLine(string.Join(",", values.Select(EscapeCsvField)));
    }

    private static string EscapeCsvField(string? value)
    {
        value ??= string.Empty;

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
