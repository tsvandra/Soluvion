using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Services;

namespace Soluvion.ViewModels
{
    public class NewAppointmentViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Service _selectedService;
        private DateTime _appointmentDate;
        private TimeSpan _appointmentTime;
        private string _notes;
        private string _errorMessage;
        private bool _hasErrorMessage;
        private ObservableCollection<Service> _services;

        public Service SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
            }
        }

        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                _appointmentDate = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set
            {
                _appointmentTime = value;
                OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Service> Services
        {
            get => _services;
            set
            {
                _services = value;
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

        public ICommand CreateAppointmentCommand { get; }

        public NewAppointmentViewModel()
        {
            _databaseService = new DatabaseService();
            CreateAppointmentCommand = new Command(async () => await OnCreateAppointmentAsync());

            // Alapértelmezett értékek
            AppointmentDate = DateTime.Today;
            AppointmentTime = TimeSpan.FromHours(9); // Alapértelmezett időpont: 9:00

            // Szolgáltatások betöltése
            LoadServicesAsync();
        }

        private async void LoadServicesAsync()
        {
            try
            {
                var services = await _databaseService.GetAllServicesAsync();
                Services = new ObservableCollection<Service>(services);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Szolgáltatások betöltése sikertelen: {ex.Message}";
            }
        }

        private async Task OnCreateAppointmentAsync()
        {
            // Validáció
            if (SelectedService == null)
            {
                ErrorMessage = "Szolgáltatás kiválasztása kötelező!";
                return;
            }

            try
            {
                var newAppointment = new Appointment
                {
                    CustomerId = 1, // Ezt később dinamikusan kell beállítani a bejelentkezett felhasználó alapján
                    ServiceId = SelectedService.Id,
                    AppointmentDate = AppointmentDate.Add(AppointmentTime),
                    StatusId = 1, // Alapértelmezett státusz: Pending
                    Notes = Notes,
                    CreatedAt = DateTime.Now
                };

                bool success = await _databaseService.CreateAppointmentAsync(newAppointment);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Sikeres foglalás",
                        "Az időpont foglalása sikeres volt!", "OK");

                    // Vissza a főoldalra
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    ErrorMessage = "Az időpont foglalása során hiba történt. Kérjük, próbáld újra!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Az időpont foglalása során hiba történt: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}