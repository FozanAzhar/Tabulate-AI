namespace TabulateAI.Api.Options;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string GeminiApiKey { get; set; } = string.Empty;

    public string MistralApiKey { get; set; } = string.Empty;

    public string GeminiModel { get; set; } = "gemini-2.5-flash";

    public string MistralOcrModel { get; set; } = "mistral-ocr-latest";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(GeminiApiKey) &&
        !string.IsNullOrWhiteSpace(MistralApiKey);
}
