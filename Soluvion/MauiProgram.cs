﻿using Microsoft.Extensions.Logging;
using Soluvion.Services;
using Soluvion.ViewModels;
using Soluvion.Views;
using Soluvion.ViewModels.Customer;
using Soluvion.ViewModels.SalonEmployee;
using Microsoft.Extensions.DependencyInjection;

namespace Soluvion;

public static class MauiProgram
{
    //public static MauiApp App { get; private set; }
    public static string ConnectionString
    {
        get
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return "Server=10.0.2.2,1433;Database=Soluvion;User Id=tsvandra;Password=.tomAsk08.;TrustServerCertificate=True";
            }
            else
            {
                return "Server=Nitro;Database=Soluvion;User Id=tsvandra;Password=.tomAsk08.;TrustServerCertificate=True";
            }
        }
    }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CustomerDashboardViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<NewAppointmentViewModel>();
        builder.Services.AddTransient<SalonDashboardViewModel>();

        //builder.Services.AddSingleton<DatabaseService>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<NewAppointmentPage>();

        builder.Services.AddTransient<Views.Customer.CustomerDashboardPage>();
        builder.Services.AddTransient<Views.SalonEmployee.SalonDashboardPage>();

        builder.Services.AddTransient<Views.Admin.AdminDashboardPage>();

        builder.Services.AddSingleton<UserService>(sp => new UserService(ConnectionString));
        builder.Services.AddSingleton<AppointmentService>(sp => new AppointmentService(ConnectionString));
        builder.Services.AddSingleton<LoginViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}


