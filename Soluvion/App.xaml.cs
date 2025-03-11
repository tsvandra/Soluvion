using Soluvion.ViewModels;
using Soluvion.Views;
using Microsoft.Maui.Controls;

namespace Soluvion
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var loginViewModel = MauiProgram.CreateMauiApp().Services.GetService<LoginViewModel>();
            MainPage = new NavigationPage(new LoginPage(loginViewModel));
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new NavigationPage(new LoginPage()));
        //} 
    }
}