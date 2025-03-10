using Soluvion.Models;
using Soluvion.ViewModels.Customer;

namespace Soluvion.Views.Customer
{
    public partial class CustomerDashboardPage : ContentPage
    {
        public CustomerDashboardPage(User user)
        {
            InitializeComponent();
            BindingContext = new CustomerDashboardViewModel(user);
        }
    }
}