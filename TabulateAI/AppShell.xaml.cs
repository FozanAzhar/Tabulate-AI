using TabulateAI.Views;

namespace TabulateAI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("ReviewReceipt", typeof(ReviewReceiptPage));
		Routing.RegisterRoute("Processing", typeof(ProcessingPage));
	}
}
