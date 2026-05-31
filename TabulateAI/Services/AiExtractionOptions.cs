namespace TabulateAI.Services;

public sealed class AiExtractionOptions
{
    public string? ApiBaseUrl { get; init; }

    public bool IsCloudEnabled => !string.IsNullOrWhiteSpace(ApiBaseUrl);

    public static AiExtractionOptions Load()
    {
        var apiBaseUrl = Environment.GetEnvironmentVariable("TABULATE_AI_API_URL");
        return new AiExtractionOptions
        {
            ApiBaseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? null : apiBaseUrl.Trim().TrimEnd('/')
        };
    }
}
