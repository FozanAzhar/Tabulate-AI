using TabulateAI.Models;

namespace TabulateAI.Services;

public interface ILocationCaptureService
{
    Task<CapturedLocation?> TryCaptureCurrentLocationAsync(CancellationToken cancellationToken = default);
}
