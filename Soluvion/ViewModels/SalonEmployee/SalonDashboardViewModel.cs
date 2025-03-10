using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Services;
using Soluvion.Views;

namespace Soluvion.ViewModels.SalonEmployee
{
    public class SalonDashboardViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;
        private readonly User _currentUser;
        private ObservableCollection<Appointment> _appointments;
        private bool _isLoading;
        private string _errorMessage;
        private bool _hasErrorMessage;
        private Appointment _selectedAppointment;

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

        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;
                OnPropertyChanged();
                OnAppointmentSelected();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ConfirmAppointmentCommand { get; }
        public ICommand CancelAppointmentCommand { get; }
        public ICommand CompleteAppointmentCommand { get; }
        public ICommand NavigateToNewAppointmentCommand { get; }

        public SalonDashboardViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _appointmentService = new AppointmentService(MauiProgram.ConnectionString);

            Appointments = new ObservableCollection<Appointment>();

            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());
            ConfirmAppointmentCommand = new Command<Appointment>(async (appointment) => await UpdateAppointmentStatus(appointment, 2)); // 2 = Confirmed
            CancelAppointmentCommand = new Command<Appointment>(async (appointment) => await UpdateAppointmentStatus(appointment, 4)); // 4 = Cancelled
            CompleteAppointmentCommand = new Command<Appointment>(async (appointment) => await UpdateAppointmentStatus(appointment, 3)); // 3 = Completed
            NavigateToNewAppointmentCommand = new Command(OnNavigateToNewAppointment);

            // Betöltjük az időpontokat
            LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var appointments = await _appointmentService.GetAppointmentsForEmployeeAsync(_currentUser.Id);

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

        private async Task UpdateAppointmentStatus(Appointment appointment, int statusId)
        {
            if (appointment == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                bool success = await _appointmentService.UpdateAppointmentStatusAsync(appointment.Id, statusId, _currentUser.Id);

                if (success)
                {
                    // Frissítjük a helyi változatot
                    appointment.StatusId = statusId;

                    // Újratöltjük az egész listát, hogy minden adat friss legyen
                    await LoadAppointmentsAsync();
                }
                else
                {
                    ErrorMessage = "Az időpont státuszának módosítása sikertelen.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Hiba történt: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAppointmentSelected()
        {
            // Itt kezelheted a kiválasztott időpontot, pl. megjelenítés külön oldalon
            if (_selectedAppointment != null)
            {
                // Például: ShowAppointmentDetails(_selectedAppointment);
            }
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