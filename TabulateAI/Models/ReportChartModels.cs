namespace TabulateAI.Models;

public class StackedBarSegment
{
    public Color Color { get; set; } = Colors.Gray;

    public double Share { get; set; }
}

public class ReportLegendItem
{
    public string Label { get; set; } = string.Empty;

    public Color Color { get; set; } = Colors.Gray;
}
