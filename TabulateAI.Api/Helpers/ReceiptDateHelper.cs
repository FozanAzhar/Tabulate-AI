using System.Globalization;
using System.Text.RegularExpressions;

namespace TabulateAI.Api.Helpers;

public static partial class ReceiptDateHelper
{
    private static readonly string[] PreferredFormats =
    [
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "d-M-yyyy",
        "dd/MM/yy",
        "d/M/yy",
        "MM/dd/yyyy",
        "M/d/yyyy"
    ];

    public static DateTime? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (DateTime.TryParseExact(trimmed, PreferredFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return Normalize(exact);
        }

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var invariant))
        {
            return Normalize(invariant);
        }

        return null;
    }

    public static DateTime? ParseFromOcrText(string? ocrText)
    {
        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return null;
        }

        foreach (Match match in OcrDateRegex().Matches(ocrText))
        {
            var candidate = Parse(match.Value);
            if (candidate.HasValue)
            {
                return candidate;
            }
        }

        return null;
    }

    public static DateTime? Normalize(DateTime value)
    {
        var date = value.Date;
        var today = DateTime.Today;
        var earliest = today.AddYears(-10);
        var latest = today.AddDays(7);
        return date >= earliest && date <= latest ? date : null;
    }

    [GeneratedRegex(@"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|\d{4}[/-]\d{1,2}[/-]\d{1,2})\b")]
    private static partial Regex OcrDateRegex();
}
