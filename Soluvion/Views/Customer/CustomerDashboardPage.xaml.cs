using Soluvion.Models;

namespace Soluvion.Views.Customer;

public partial class CustomerDashboardPage : ContentPage
{
    public CustomerDashboardPage()
    {
        InitializeComponent();
    }

    public CustomerDashboardPage(User user)
    {
        InitializeComponent();
        UserNameLabel.Text = $"Hello, {user.Name}!";
    }
}