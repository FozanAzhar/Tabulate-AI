namespace TabulateAI.Api.Options;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string GeminiApiKey { get; set; } = string.Empty;

    public string MistralApiKey { get; set; } = string.Empty;

    public string GeminiModel { get; set; } = "gemini-2.5-flash";

    public string MistralOcrModel { get; set; } = "mistral-ocr-latest";

    public bool HasGemini => !string.IsNullOrWhiteSpace(GeminiApiKey);

    public bool HasMistral => !string.IsNullOrWhiteSpace(MistralApiKey);

    /// <summary>
    /// Gemini is required. Mistral is optional and used only as an OCR fallback.
    /// </summary>
    public bool IsConfigured => HasGemini;
}
