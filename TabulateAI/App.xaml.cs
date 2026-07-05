using TabulateAI.Services;
using TabulateAI.Views;

namespace TabulateAI;

public partial class App : Application
{
	public App(IAppSettingsService appSettings)
	{
		InitializeComponent();
		appSettings.Load();

		AppDomain.CurrentDomain.UnhandledException += (_, args) =>
		{
			WriteCrashLog(args.ExceptionObject?.ToString() ?? "Unknown unhandled exception");
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell())
		{
			Title = "Tabulate AI",
			Width = 420,
			Height = 780,
			X = 100,
			Y = 50
		};

#if WINDOWS
		window.Created += (_, _) =>
		{
			if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
			{
				nativeWindow.Activate();
			}
		};
#endif

		return window;
	}

	internal static void WriteCrashLog(string message)
	{
		try
		{
			var path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Expensely",
				"crash.log");
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.AppendAllText(path, $"{DateTime.Now:O}{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}");
		}
		catch
		{
			// Best-effort logging only.
		}
	}
}
