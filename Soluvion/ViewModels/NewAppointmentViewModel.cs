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
        private readonly AppointmentService _appointmentService;
        private readonly UserService _userService;
        private User _selectedCustomer;
        private Service _selectedService;
        private DateTime _appointmentDate;
        private TimeSpan _appointmentTime;
        private string _notes;
        private string _errorMessage;
        private bool _hasErrorMessage;
        private ObservableCollection<User> _customers;
        private ObservableCollection<Service> _services;

        public User SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
            }
        }

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

        public ObservableCollection<User> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
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
            _appointmentService = new AppointmentService(MauiProgram.ConnectionString);
            _userService = new UserService(MauiProgram.ConnectionString);
            CreateAppointmentCommand = new Command(async () => await OnCreateAppointmentAsync());

            // Alapértelmezett értékek
            AppointmentDate = DateTime.Today;
            AppointmentTime = TimeSpan.FromHours(9); // Alapértelmezett időpont: 9:00

            // Szolgáltatások és ügyfelek betöltése
            LoadServicesAsync();
            LoadCustomersAsync();
        }

        private async void LoadServicesAsync()
        {
            try
            {
                var services = await _appointmentService.GetAllServicesAsync();
                Services = new ObservableCollection<Service>(services);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Szolgáltatások betöltése sikertelen: {ex.Message}";
            }
        }

        private async void LoadCustomersAsync()
        {
            try
            {
                var customers = await _userService.GetAllUsersAsync();
                Customers = new ObservableCollection<User>(customers);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ügyfelek betöltése sikertelen: {ex.Message}";
            }
        }

        private async Task OnCreateAppointmentAsync()
        {
            // Validáció
            if (SelectedCustomer == null)
            {
                ErrorMessage = "Ügyfél kiválasztása kötelező!";
                return;
            }

            if (SelectedService == null)
            {
                ErrorMessage = "Szolgáltatás kiválasztása kötelező!";
                return;
            }

            try
            {
                var newAppointment = new Appointment
                {
                    CustomerId = SelectedCustomer.Id,
                    ServiceId = SelectedService.Id,
                    AppointmentDate = AppointmentDate.Add(AppointmentTime),
                    StatusId = 1, // Alapértelmezett státusz: Pending
                    Notes = Notes,
                    CreatedAt = DateTime.Now
                };

                bool success = await _appointmentService.CreateAppointmentAsync(newAppointment);

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