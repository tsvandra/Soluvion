using System.Collections.ObjectModel;
using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Services;
using Soluvion.Views;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Soluvion.ViewModels.Customer
{
    public class CustomerDashboardViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;
        private User _currentUser;
        private ObservableCollection<Appointment> _appointments;
        private bool _isLoading;
        private string _errorMessage;
        private bool _hasErrorMessage;

        public CustomerDashboardViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            Appointments = new ObservableCollection<Appointment>();

            NavigateToNewAppointmentCommand = new Command(async () => await OnNavigateToNewAppointmentAsync());
            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        /// <summary>
        /// Call this method from the view's OnAppearing method.
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadAppointmentsAsync();
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

        private async Task OnNavigateToNewAppointmentAsync()
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