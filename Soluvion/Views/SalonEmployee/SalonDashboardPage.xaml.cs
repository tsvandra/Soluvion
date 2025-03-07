using Soluvion.Models;
using Soluvion.ViewModels.SalonEmployee;

namespace Soluvion.Views.SalonEmployee
{
    public partial class SalonDashboardPage : ContentPage
    {
        private readonly SalonDashboardViewModel _viewModel;

        public SalonDashboardPage(User user)
        {
            InitializeComponent();

            _viewModel = new SalonDashboardViewModel(user);
            BindingContext = _viewModel;

            UserNameLabel.Text = $"Üdvözöljük, {user.Name}!";
        }
    }
}
