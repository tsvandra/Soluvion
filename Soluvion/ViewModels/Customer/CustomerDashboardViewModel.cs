using System.Collections.ObjectModel;
using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Services;
using Soluvion.Views;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Soluvion.ViewModels.Customer
{
    public class CustomerDashboardViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;
        private readonly User _currentUser;
        private ObservableCollection<Appointment> _appointments;
        private bool _isLoading;
        private string _errorMessage;
        private bool _hasErrorMessage;

        public CustomerDashboardViewModel()
        {

        }

        public ObservableCollection<Appointment> Appointments
        {
            get => _appointments;
            set
            {
                _appointments = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                HasErrorMessage = !string.IsNullOrEmpty(value);
                OnPropertyChanged();
            }
        }

        public bool HasErrorMessage
        {
            get => _hasErrorMessage;
            set
            {
                _hasErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToNewAppointmentCommand { get; }
        public ICommand RefreshCommand { get; }

        public CustomerDashboardViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _appointmentService = new AppointmentService(MauiProgram.ConnectionString);
            Appointments = new ObservableCollection<Appointment>();

            NavigateToNewAppointmentCommand = new Command(OnNavigateToNewAppointment);
            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());

            // Betöltjük az időpontokat
            LoadAppointmentsAsync();
        }

        private async void OnNavigateToNewAppointment()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new NewAppointmentPage());
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var appointments = await _appointmentService.GetAppointmentsForCustomerAsync(_currentUser.Id);

                Appointments.Clear();
                foreach (var appointment in appointments)
                {
                    Appointments.Add(appointment);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Időpontok betöltése sikertelen: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}