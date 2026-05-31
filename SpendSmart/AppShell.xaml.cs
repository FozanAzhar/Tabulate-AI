using SpendSmart.Views;

namespace SpendSmart;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("ReviewReceipt", typeof(ReviewReceiptPage));
	}
}
