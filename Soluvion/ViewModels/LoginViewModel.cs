using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Soluvion.Models;
using Microsoft.Maui.Controls;
using Soluvion.Views;
using Soluvion.Services;
using Soluvion.ViewModels.Customer;

namespace Soluvion.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private readonly UserService _userService;
        private bool _isDebugMode;

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

        public bool IsDebugMode
        {
            get => _isDebugMode;
            set
            {
                _isDebugMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand DebugLoginCommand { get; }

        public LoginViewModel(UserService userService)
        {
            _userService = userService;
            LoginCommand = new Command(async () => await OnLoginAsync());
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
            DebugLoginCommand = new Command<string>(async (role) => await OnDebugLoginAsync(role));

#if DEBUG
            IsDebugMode = true;
#else
            IsDebugMode = false;
#endif
        }

        private async Task OnLoginAsync()
        {
            try
            {
                bool isValid = await _userService.ValidateUserAsync(Username, Password);

                if (isValid)
                {
                    var user = await _userService.GetUserAsync(Username);
                    Preferences.Set("CurrentUserRoleId", user.RoleId);
                    // Navigálás a megfelelõ oldalra a szerepkörtõl függõen
                    switch (user.Role)
                    {
                        case "Admin":
                            await Application.Current.MainPage.Navigation.PushAsync(new Views.Admin.AdminDashboardPage(user));
                            break;
                        case "SalonEmployee":
                            await Application.Current.MainPage.Navigation.PushAsync(new Views.SalonEmployee.SalonDashboardPage(user));
                            break;
                        case "Customer":
                            var customerDashboardViewModel = MauiProgram.CreateMauiApp().Services.GetService<CustomerDashboardViewModel>();
                            await Application.Current.MainPage.Navigation.PushAsync(new Views.Customer.CustomerDashboardPage(customerDashboardViewModel, user));
                            break;
                        default:
                            await Application.Current.MainPage.DisplayAlert("Access Denied", "Invalid username or password", "OK");
                            break;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Login", "Invalid username or password", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
        }

        private async Task OnDebugLoginAsync(string role)
        {
            if (role == "customer")
            {
                Username = "customer";
                Password = "cust123";
            }
            else if (role == "employee")
            {
                Username = "employee";
                Password = "emp123";
            }

            await OnLoginAsync();
        }

        private async void OnNavigateToRegister()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}