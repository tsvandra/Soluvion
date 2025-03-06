using Soluvion.Models;

namespace Soluvion.Views.Admin;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage()
    {
        InitializeComponent();
    }

    public AdminDashboardPage(User user)
    {
        InitializeComponent();
        UserNameLabel.Text = $"Hello, {user.Name}!";
    }
}