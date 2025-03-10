using Soluvion.Models;
using Soluvion.ViewModels.Customer;

namespace Soluvion.Views.Customer
{
    public partial class CustomerDashboardPage : ContentPage
    {
        public CustomerDashboardPage(CustomerDashboardViewModel viewModel, User user)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.SetCurrentUser(user);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CustomerDashboardViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
    }
}