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
        var exportDirectory = Path.Combine(FileSystem.CacheDirectory, "exports");
        Directory.CreateDirectory(exportDirectory);

        var fileName = $"Expensely_Report_{fileToken}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(exportDirectory, fileName);

        // UTF-8 BOM helps Excel and Google Sheets detect columns correctly.
        await using var stream = File.Create(filePath);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await writer.WriteAsync(csvContent);

        return filePath;
    }

    public Task ShareCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Export file was not found.", filePath);
        }

        return Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Export expense report",
            File = new ShareFile(filePath, "text/csv")
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
