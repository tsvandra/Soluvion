using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Soluvion.Models;
using Microsoft.Maui.Controls;
using Soluvion.Views;
using Soluvion.Services;

namespace Soluvion.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private readonly DatabaseService _databaseService;

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

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            _databaseService = new DatabaseService();
            LoginCommand = new Command(async () => await OnLoginAsync());
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
        }

        private async Task OnLoginAsync()
        {
            try
            {
                bool isValid = await _databaseService.ValidateUserAsync(Username, Password);

                if (isValid)
                {
                    var user = await _databaseService.GetUserAsync(Username);

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
                            await Application.Current.MainPage.Navigation.PushAsync(new Views.Customer.CustomerDashboardPage(user));
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnNavigateToRegister()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }
    }
}
