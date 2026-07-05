namespace TabulateAI.Helpers;

public static class ReportPeriodHelper
{
    public record PeriodRange(DateTime Start, DateTime End, string Label, string FileToken);

    public static PeriodRange Resolve(
        string selectedPeriod,
        DateTime? customStart = null,
        DateTime? customEnd = null,
        DateTime? referenceDate = null)
    {
        var today = (referenceDate ?? DateTime.Now).Date;

        return selectedPeriod switch
        {
            "Last month" or "Last Month" => ResolveLastMonth(today),
            "This year" or "This Year" => ResolveThisYear(today),
            "Custom" when customStart.HasValue && customEnd.HasValue => ResolveCustom(customStart.Value, customEnd.Value),
            _ => ResolveThisMonth(today)
        };
    }

    private static PeriodRange ResolveThisYear(DateTime today)
    {
        var start = new DateTime(today.Year, 1, 1);
        var end = today;
        var label = $"{today.Year} YTD".ToUpperInvariant();
        return new PeriodRange(start, end, label, $"{today.Year}YTD");
    }

    private static PeriodRange ResolveThisMonth(DateTime today)
    {
        var start = new DateTime(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new PeriodRange(start, end, start.ToString("MMMM yyyy").ToUpperInvariant(), start.ToString("yyyyMM"));
    }

    private static PeriodRange ResolveLastMonth(DateTime today)
    {
        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var end = start.AddMonths(1).AddDays(-1);
        return new PeriodRange(start, end, start.ToString("MMMM yyyy").ToUpperInvariant(), start.ToString("yyyyMM"));
    }

    private static PeriodRange ResolveCustom(DateTime customStart, DateTime customEnd)
    {
        var start = customStart.Date;
        var end = customEnd.Date;
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var label = start.Year == end.Year && start.Month == end.Month
            ? start.ToString("MMMM yyyy").ToUpperInvariant()
            : $"{start:dd MMM yyyy} – {end:dd MMM yyyy}".ToUpperInvariant();

        var fileToken = $"{start:yyyyMMdd}_{end:yyyyMMdd}";
        return new PeriodRange(start, end, label, fileToken);
    }
}
