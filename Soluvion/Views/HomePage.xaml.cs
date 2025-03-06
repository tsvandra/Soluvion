using Soluvion.Models;

namespace Soluvion.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    public HomePage(User user)
    {
        InitializeComponent();
        Title = $"Hello, {user.Name}!";
    }
}