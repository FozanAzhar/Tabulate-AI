using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TabulateAI.Api.Options;

namespace TabulateAI.Api.Security;

public sealed class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiSecurityOptions _options;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IOptions<ApiSecurityOptions> options,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresApiKey(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!_options.IsApiKeyConfigured)
        {
            _logger.LogWarning("Rejecting protected request: Security:ClientApiKey is not configured.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { title = "API security not configured" });
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var providedKey) ||
            !CryptographicEquals(providedKey.ToString(), _options.ClientApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Invalid or missing API key" });
            return;
        }

        await _next(context);
    }

    private static bool RequiresApiKey(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

    private static bool CryptographicEquals(string? a, string? b)
    {
        if (a is null || b is null)
        {
            return false;
        }

        var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
        var bBytes = System.Text.Encoding.UTF8.GetBytes(b);

        if (aBytes.Length != bBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
