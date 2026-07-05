namespace TabulateAI.Models;

public sealed class CapturedLocation
{
    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string Address { get; init; } = string.Empty;
}
