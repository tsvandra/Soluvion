using Soluvion.Models;

namespace Soluvion.Views.SalonEmployee;

public partial class SalonDashboardPage : ContentPage
{
    public SalonDashboardPage()
    {
        InitializeComponent();
    }

    public SalonDashboardPage(User user)
    {
        InitializeComponent();
        UserNameLabel.Text = $"Hello, {user.Name}!";
    }
}