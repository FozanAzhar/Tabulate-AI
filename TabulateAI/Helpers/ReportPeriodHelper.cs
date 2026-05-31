namespace TabulateAI.Helpers;

public static class ReportPeriodHelper
{
    public record PeriodRange(DateTime Start, DateTime End, string Label, string FileToken);

    public static PeriodRange Resolve(string selectedPeriod, DateTime? referenceDate = null)
    {
        var now = referenceDate ?? DateTime.Now;
        var today = now.Date;

        return selectedPeriod switch
        {
            "Last Month" => ResolveLastMonth(today),
            "Year" => ResolveYear(today),
            _ => ResolveThisMonth(today)
        };
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

    private static PeriodRange ResolveYear(DateTime today)
    {
        var start = new DateTime(today.Year, 1, 1);
        var end = new DateTime(today.Year, 12, 31);
        return new PeriodRange(start, end, today.Year.ToString(), today.Year.ToString());
    }
}
