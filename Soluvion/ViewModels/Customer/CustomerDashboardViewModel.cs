using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Views;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Soluvion.ViewModels.Customer
{
    public class CustomerDashboardViewModel : INotifyPropertyChanged
    {
        private readonly User _currentUser;

        public ICommand NavigateToNewAppointmentCommand { get; }

        public CustomerDashboardViewModel()
        {
        }

        public CustomerDashboardViewModel(User currentUser)
        {
            _currentUser = currentUser;
            NavigateToNewAppointmentCommand = new Command(OnNavigateToNewAppointment);
        }

        private async void OnNavigateToNewAppointment()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new NewAppointmentPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}