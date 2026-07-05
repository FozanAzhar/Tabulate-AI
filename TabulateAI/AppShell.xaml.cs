using TabulateAI.Helpers;
using TabulateAI.Views;



#if ANDROID

using Android.Views;

using Google.Android.Material.BottomNavigation;

using Microsoft.Maui.Platform;

#endif



namespace TabulateAI;



public partial class AppShell : Shell

{

	public AppShell()

	{

		InitializeComponent();

		Routing.RegisterRoute("processing", typeof(ProcessingPage));

		Routing.RegisterRoute("receiptdetail", typeof(ReviewReceiptPage));

		Routing.RegisterRoute("manualexpense", typeof(ManualExpensePage));
		Routing.RegisterRoute("exportpreview", typeof(ExportPreviewPage));
		Routing.RegisterRoute("emailreportpreview", typeof(EmailReportPreviewPage));

		Routing.RegisterRoute("Processing", typeof(ProcessingPage));

		Routing.RegisterRoute("ReviewReceipt", typeof(ReviewReceiptPage));



		Loaded += OnShellLoaded;

	}



	private async void OnShellLoaded(object? sender, EventArgs e)
	{
		ThemeResourceHelper.ApplyShellColors();

#if ANDROID
		await Task.Delay(150);
		FixAndroidTabBar();
		ThemeResourceHelper.ApplyShellColors();
#endif
	}



#if ANDROID

	private static void FixAndroidTabBar()

	{

		if (Platform.CurrentActivity?.Window?.DecorView is not ViewGroup root)

		{

			return;

		}



		var bottomNav = FindChild<BottomNavigationView>(root);

		if (bottomNav is null)

		{

			return;

		}



		bottomNav.Post(() =>

		{

			bottomNav.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;

			bottomNav.ItemIconSize = (int)(24 * bottomNav.Resources!.DisplayMetrics!.Density);



			var layoutParams = bottomNav.LayoutParameters;

			if (layoutParams is not null)

			{

				layoutParams.Height = (int)(68 * bottomNav.Resources.DisplayMetrics.Density);

				bottomNav.LayoutParameters = layoutParams;

			}



			bottomNav.SetPadding(0, (int)(4 * bottomNav.Resources.DisplayMetrics.Density), 0, 0);

		});

	}



	private static T? FindChild<T>(ViewGroup parent) where T : Android.Views.View

	{

		for (var i = 0; i < parent.ChildCount; i++)

		{

			if (parent.GetChildAt(i) is T match)

			{

				return match;

			}



			if (parent.GetChildAt(i) is ViewGroup group)

			{

				var found = FindChild<T>(group);

				if (found is not null)

				{

					return found;

				}

			}

		}



		return null;

	}

#endif

}

