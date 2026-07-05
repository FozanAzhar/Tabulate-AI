using System.Globalization;
using Microsoft.Extensions.Logging;
using TabulateAI.Helpers;
using TabulateAI.Models;

namespace TabulateAI.Services;

public sealed class LocationCaptureService : ILocationCaptureService
{
    private readonly ILogger<LocationCaptureService> _logger;

    public LocationCaptureService(ILogger<LocationCaptureService> logger)
    {
        _logger = logger;
    }

    public async Task<CapturedLocation?> TryCaptureCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

        try
        {
            return await CaptureCoreAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Location capture timed out.");
            return null;
        }
    }

    private async Task<CapturedLocation?> CaptureCoreAsync(CancellationToken cancellationToken)
    {
        if (!await LocationPermissionHelper.EnsureWhenInUseAsync())
        {
            _logger.LogInformation("Location permission not granted.");
            return null;
        }

        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(5)
            }, cancellationToken);

            if (location is null)
            {
                return null;
            }

            var address = await ResolveAddressAsync(location, cancellationToken);
            return new CapturedLocation
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Address = address
            };
        }
        catch (FeatureNotSupportedException ex)
        {
            _logger.LogWarning(ex, "Geolocation is not supported on this device.");
            return null;
        }
        catch (PermissionException ex)
        {
            _logger.LogWarning(ex, "Location permission denied.");
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture current location.");
            return null;
        }
    }

    private static async Task<string> ResolveAddressAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            var geocodeTask = Geocoding.Default.GetPlacemarksAsync(
                location.Latitude,
                location.Longitude);

            var completed = await Task.WhenAny(geocodeTask, Task.Delay(TimeSpan.FromSeconds(4), cancellationToken));
            if (completed != geocodeTask)
            {
                return FormatCoordinates(location);
            }

            var placemarks = await geocodeTask;
            var placemark = placemarks?.FirstOrDefault();
            if (placemark is not null)
            {
                var formatted = FormatPlacemark(placemark);
                if (!string.IsNullOrWhiteSpace(formatted))
                {
                    return formatted;
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Fall back to coordinates below.
        }

        return FormatCoordinates(location);
    }

    private static string FormatCoordinates(Location location) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{location.Latitude:F5}, {location.Longitude:F5}");

    private static string FormatPlacemark(Placemark placemark)
    {
        var parts = new[]
        {
            placemark.FeatureName,
            placemark.Thoroughfare,
            placemark.SubThoroughfare,
            placemark.Locality,
            placemark.AdminArea,
            placemark.PostalCode
        }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .Select(part => part!.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join(", ", parts);
    }
}
