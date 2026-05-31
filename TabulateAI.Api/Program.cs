using Microsoft.Extensions.Options;
using TabulateAI.Api.Models;
using TabulateAI.Api.Options;
using TabulateAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.AddHttpClient<MistralOcrClient>();
builder.Services.AddHttpClient<GeminiReceiptParser>();
builder.Services.AddSingleton<ReceiptAiPipeline>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "Tabulate-AI API",
    pipeline = "Mistral OCR -> Gemini 2.5 Flash",
    configured = app.Services.GetRequiredService<IOptions<AiOptions>>().Value.IsConfigured
}));

app.MapPost("/api/receipts/extract", async (HttpRequest request, ReceiptAiPipeline pipeline, IOptions<AiOptions> options, CancellationToken cancellationToken) =>
{
    if (!options.Value.IsConfigured)
    {
        return Results.Problem(
            title: "AI keys not configured",
            detail: "Set Ai:GeminiApiKey and Ai:MistralApiKey in user secrets or appsettings.Development.json.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Expected multipart/form-data with an image file field named 'file'.");
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest("Missing file upload.");
    }

    await using var stream = file.OpenReadStream();
    using var memory = new MemoryStream();
    await stream.CopyToAsync(memory, cancellationToken);

    var result = await pipeline.ExtractAsync(memory.ToArray(), file.ContentType, cancellationToken);
    return Results.Ok(result);
})
.WithName("ExtractReceipt");

app.Run();
