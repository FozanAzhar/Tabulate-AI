using System.Globalization;
using System.Text;
using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class EmailReportTemplateHelper
{
    public record EmailDraft(string Subject, string Body, string AttachmentLabel);

    public static EmailDraft Build(
        IReadOnlyList<Receipt> receipts,
        string periodLabel,
        string senderName,
        string? customBody = null)
    {
        var total = receipts.Sum(r => r.Amount);
        var count = receipts.Count;
        var totalFormatted = total.ToString("C2", CultureInfo.CurrentCulture);
        var greetingName = string.IsNullOrWhiteSpace(senderName) ? "there" : senderName.Trim();

        var topCategory = receipts
            .GroupBy(r => r.Category)
            .OrderByDescending(g => g.Sum(r => r.Amount))
            .Select(g => new { Category = g.Key, Total = g.Sum(r => r.Amount) })
            .FirstOrDefault();

        var subject = $"Expense report – {periodLabel} – {totalFormatted}";

        if (!string.IsNullOrWhiteSpace(customBody))
        {
            return new EmailDraft(
                subject,
                customBody,
                BuildAttachmentLabel(periodLabel, count, totalFormatted));
        }

        var body = new StringBuilder();
        body.AppendLine("Hi,");
        body.AppendLine();
        body.AppendLine($"Please find attached my expense report for {periodLabel}.");
        body.AppendLine();
        body.AppendLine("Summary");
        body.AppendLine($"• Total spending: {totalFormatted}");
        body.AppendLine($"• Receipts included: {count}");
        body.AppendLine($"• Report period: {periodLabel}");

        if (topCategory is not null && total > 0)
        {
            var share = topCategory.Total / total;
            body.AppendLine($"• Top category: {topCategory.Category} ({share:P0} of total)");
        }

        body.AppendLine();
        body.AppendLine("The attached PDF includes receipt thumbnails for reimbursement or tax records.");
        body.AppendLine();
        body.AppendLine("Kind regards,");
        body.AppendLine(greetingName);

        return new EmailDraft(subject, body.ToString().TrimEnd(), BuildAttachmentLabel(periodLabel, count, totalFormatted));
    }

    private static string BuildAttachmentLabel(string periodLabel, int count, string totalFormatted) =>
        $"Expensely_Report_{periodLabel.Replace(' ', '_')}.pdf · {count} receipt{(count == 1 ? string.Empty : "s")} · {totalFormatted}";
}
