namespace TabulateAI.Services;

public sealed class AiExtractionOptions
{
    public const string ApiKeyHeaderName = "X-Api-Key";

    public string? ApiBaseUrl { get; init; }

    public string? ApiKey { get; init; }

    public bool IsCloudEnabled => !string.IsNullOrWhiteSpace(ApiBaseUrl);

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    public static AiExtractionOptions Load()
    {
        var apiBaseUrl = Environment.GetEnvironmentVariable("TABULATE_AI_API_URL");
        var apiKey = Environment.GetEnvironmentVariable("TABULATE_AI_API_KEY");

#if DEBUG && ANDROID
        apiBaseUrl ??= "http://10.0.2.2:5299";
#endif

        return new AiExtractionOptions
        {
            ApiBaseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? null : apiBaseUrl.Trim().TrimEnd('/'),
            ApiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim()
        };
    }
}
