using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TabulateAI.Api.Models;
using TabulateAI.Api.Options;
using TabulateAI.Api.Security;
using TabulateAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var securitySection = builder.Configuration.GetSection(ApiSecurityOptions.SectionName);
builder.Services.Configure<ApiSecurityOptions>(securitySection);
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));

var securityOptions = securitySection.Get<ApiSecurityOptions>() ?? new ApiSecurityOptions();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = securityOptions.MaxUploadBytes;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = securityOptions.MaxUploadBytes;
});

builder.Services.AddHttpClient<MistralOcrClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});
builder.Services.AddHttpClient<GeminiReceiptParser>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddSingleton<ReceiptAiPipeline>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("extract", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = Math.Max(1, securityOptions.RateLimitPerMinute),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true);
            return;
        }

        var origins = securityOptions.AllowedOrigins;
        if (origins.Length == 0)
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .DisallowCredentials();
            return;
        }

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.MapGet("/", () => Results.Ok(new
{
    service = "Tabulate-AI API",
    status = "ok",
    configured = app.Services.GetRequiredService<IOptions<AiOptions>>().Value.IsConfigured,
    secured = app.Services.GetRequiredService<IOptions<ApiSecurityOptions>>().Value.IsApiKeyConfigured
}));

app.MapPost("/api/receipts/extract", async (HttpRequest request, ReceiptAiPipeline pipeline, IOptions<AiOptions> options, IOptions<ApiSecurityOptions> security, CancellationToken cancellationToken) =>
{
    if (!options.Value.IsConfigured)
    {
        return Results.Problem(
            title: "AI keys not configured",
            detail: "Set Ai:GeminiApiKey in user secrets or appsettings.Development.json. Ai:MistralApiKey is optional fallback.",
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

    if (file.Length > security.Value.MaxUploadBytes)
    {
        return Results.BadRequest($"File exceeds maximum size of {security.Value.MaxUploadBytes / (1024 * 1024)} MB.");
    }

    if (!IsAllowedImageType(file.ContentType))
    {
        return Results.BadRequest("Unsupported file type. Upload JPEG, PNG, WebP, or BMP.");
    }

    await using var stream = file.OpenReadStream();
    using var memory = new MemoryStream();
    await stream.CopyToAsync(memory, cancellationToken);

    var result = await pipeline.ExtractAsync(memory.ToArray(), file.ContentType, cancellationToken);
    return Results.Ok(result);
})
.RequireRateLimiting("extract")
.WithName("ExtractReceipt");

app.Run();

static bool IsAllowedImageType(string? contentType)
{
    if (string.IsNullOrWhiteSpace(contentType))
    {
        return false;
    }

    var type = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
    return type is "image/jpeg" or "image/png" or "image/webp" or "image/bmp";
}
