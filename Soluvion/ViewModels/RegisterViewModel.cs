using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Soluvion.Models;
using Soluvion.Services;
using Soluvion.Views;

namespace Soluvion.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _name;
        private UserRole _selectedRole;
        private string _errorMessage;
        private bool _hasErrorMessage;
        private ObservableCollection<UserRole> _roles;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public UserRole SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserRole> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
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

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel()
        {
            _databaseService = new DatabaseService();
            RegisterCommand = new Command(async () => await OnRegisterAsync());
            BackToLoginCommand = new Command(OnBackToLogin);

            // Szerepkörök betöltése
            LoadRolesAsync();
        }

        private async void LoadRolesAsync()
        {
            try
            {
                var roles = await _databaseService.GetAllUserRolesAsync();
                var availableRoles = roles.Where(r => r.RoleName != "Admin").ToList();
                Roles = new ObservableCollection<UserRole>(availableRoles);

                // Alapértelmezett szerepkör beállítása (pl. Customer)
                SelectedRole = availableRoles.FirstOrDefault(r => r.RoleName == "Customer");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Szerepkörök betöltése sikertelen: {ex.Message}";
            }
        }

        private async Task OnRegisterAsync()
        {
            // Validáció
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "A felhasználónév megadása kötelező!";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "A jelszó megadása kötelező!";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "A jelszavak nem egyeznek!";
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "A név megadása kötelező!";
                return;
            }

            if (SelectedRole == null)
            {
                ErrorMessage = "Szerepkör kiválasztása kötelező!";
                return;
            }

            try
            {
                // Ellenőrizzük, hogy létezik-e már a felhasználó
                bool userExists = await _databaseService.CheckUserExistsAsync(Username);
                if (userExists)
                {
                    ErrorMessage = "Ez a felhasználónév már foglalt!";
                    return;
                }

                // Új felhasználó létrehozása
                var newUser = new User
                {
                    Username = Username,
                    Password = Password, // Figyelem: később jelszó hash-elést kell alkalmazni
                    Name = Name,
                    RoleId = SelectedRole.Id,
                    Role = SelectedRole
                };

                bool success = await _databaseService.CreateUserAsync(newUser);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Sikeres regisztráció",
                        "A regisztráció sikeres volt! Most már bejelentkezhetsz.", "OK");

                    // Vissza a bejelentkezési oldalra
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    ErrorMessage = "A regisztráció során hiba történt. Kérjük, próbáld újra!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"A regisztráció során hiba történt: {ex.Message}";
            }
        }

        private async void OnBackToLogin()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}