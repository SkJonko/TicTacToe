namespace TicToe.Infinite.Pages;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}