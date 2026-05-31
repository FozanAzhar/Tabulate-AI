using Microsoft.Extensions.Logging;
using Microcharts.Maui;
using SpendSmart.Services;
using SpendSmart.ViewModels;
using SpendSmart.Views;

namespace SpendSmart;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMicrocharts()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
		builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();

#if WINDOWS
		builder.Services.AddSingleton<IOcrService, WindowsOcrService>();
#else
		builder.Services.AddSingleton<IOcrService, StubOcrService>();
#endif

		builder.Services.AddTransient<ScanViewModel>();
		builder.Services.AddTransient<ReviewReceiptViewModel>();
		builder.Services.AddTransient<ArchiveViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();

		builder.Services.AddTransient<ScanPage>();
		builder.Services.AddTransient<ReviewReceiptPage>();
		builder.Services.AddTransient<ArchivePage>();
		builder.Services.AddTransient<DashboardPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		var repository = app.Services.GetRequiredService<IReceiptRepository>();
		repository.InitializeAsync().GetAwaiter().GetResult();

		return app;
	}
}
