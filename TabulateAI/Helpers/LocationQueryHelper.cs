using System.Globalization;
using System.Text;
using TabulateAI.Models;

namespace TabulateAI.Helpers;

public static class LocationQueryHelper
{
    public static void AppendTo(StringBuilder query, CapturedLocation? location)
    {
        if (location is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(location.Address))
        {
            query.Append("&Address=").Append(Uri.EscapeDataString(location.Address.Trim()));
        }

        query.Append("&Latitude=")
            .Append(location.Latitude.ToString(CultureInfo.InvariantCulture));
        query.Append("&Longitude=")
            .Append(location.Longitude.ToString(CultureInfo.InvariantCulture));
    }

    public static void AppendTo(List<string> parts, string? address, string? latitudeText, string? longitudeText)
    {
        if (!string.IsNullOrWhiteSpace(address))
        {
            parts.Add($"Address={Uri.EscapeDataString(address.Trim())}");
        }

        if (double.TryParse(latitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
        {
            parts.Add($"Latitude={latitude.ToString(CultureInfo.InvariantCulture)}");
        }

        if (double.TryParse(longitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            parts.Add($"Longitude={longitude.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
