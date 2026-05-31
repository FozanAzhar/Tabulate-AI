using System.Globalization;
using System.Text.RegularExpressions;
using TabulateAI.Models;

namespace TabulateAI.Services;

public static partial class ReceiptParser
{
    public static OcrExtractionResult Parse(string rawText)
    {
        var lines = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var merchant = lines.FirstOrDefault(l => l.Length > 2) ?? string.Empty;
        var amount = ExtractAmount(rawText);
        var date = ExtractDate(rawText);

        return new OcrExtractionResult
        {
            RawText = rawText,
            Merchant = merchant,
            Amount = amount,
            Date = date,
            SuggestedCategory = CategorySuggestionService.SuggestCategory(merchant)
        };
    }

    private static decimal? ExtractAmount(string text)
    {
        var matches = AmountRegex().Matches(text);
        decimal? best = null;

        foreach (Match match in matches)
        {
            if (!decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                continue;
            }

            if (best is null || value >= best)
            {
                best = value;
            }
        }

        return best;
    }

    private static DateTime? ExtractDate(string text)
    {
        foreach (Match match in DateRegex().Matches(text))
        {
            var value = match.Value;
            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.Date;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed.Date;
            }
        }

        return null;
    }

    [GeneratedRegex(@"(\d+[.,]\d{2})")]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|\d{4}[/-]\d{1,2}[/-]\d{1,2})\b")]
    private static partial Regex DateRegex();
}
