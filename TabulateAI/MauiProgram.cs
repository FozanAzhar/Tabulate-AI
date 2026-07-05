using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TabulateAI.Services;
using TabulateAI.ViewModels;
using TabulateAI.Views;

namespace TabulateAI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansBold");
			});

		builder.Services.AddSingleton(AiExtractionOptions.Load());
		builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
		builder.Services.AddSingleton<PendingReceiptContext>();
		builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
		builder.Services.AddSingleton<IExpenseExportService, ExpenseExportService>();
		builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();
		builder.Services.AddSingleton<IBackupService, BackupService>();
		builder.Services.AddSingleton<ILocationCaptureService, LocationCaptureService>();
		builder.Services.AddHttpClient<IMerchantLogoService, MerchantLogoService>();
		builder.Services.AddHttpClient<CloudOcrService>();

#if WINDOWS
		builder.Services.AddSingleton<WindowsOcrService>();
		builder.Services.AddSingleton<ILocalOcrService>(sp => sp.GetRequiredService<WindowsOcrService>());
#else
		builder.Services.AddSingleton<StubOcrService>();
		builder.Services.AddSingleton<ILocalOcrService>(sp => sp.GetRequiredService<StubOcrService>());
#endif

		builder.Services.AddSingleton<HybridOcrService>();
		builder.Services.AddSingleton<IOcrService>(sp => sp.GetRequiredService<HybridOcrService>());

		builder.Services.AddTransient<SplashViewModel>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<ScanViewModel>();
		builder.Services.AddTransient<ProcessingViewModel>();
		builder.Services.AddTransient<ReviewReceiptViewModel>();
		builder.Services.AddTransient<ManualExpenseViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<ReportsViewModel>();
		builder.Services.AddTransient<ExportPreviewViewModel>();
		builder.Services.AddTransient<EmailReportPreviewViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();

		builder.Services.AddTransient<SplashPage>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<ScanPage>();
		builder.Services.AddTransient<ProcessingPage>();
		builder.Services.AddTransient<ReviewReceiptPage>();
		builder.Services.AddTransient<ManualExpensePage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<ReportsPage>();
		builder.Services.AddTransient<ExportPreviewPage>();
		builder.Services.AddTransient<EmailReportPreviewPage>();
		builder.Services.AddTransient<SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
