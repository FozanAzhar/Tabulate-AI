namespace TabulateAI.Api.Options;

public sealed class ApiSecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>Shared secret the mobile app sends via X-Api-Key. Required for extract requests.</summary>
    public string ClientApiKey { get; set; } = string.Empty;

    /// <summary>Max receipt image upload size in bytes (default 10 MB).</summary>
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Extract requests allowed per IP per minute.</summary>
    public int RateLimitPerMinute { get; set; } = 30;

    /// <summary>Browser origins allowed in non-Development environments (mobile apps ignore CORS).</summary>
    public string[] AllowedOrigins { get; set; } = [];

    public bool IsApiKeyConfigured => !string.IsNullOrWhiteSpace(ClientApiKey);
}
