using Microsoft.Extensions.Logging;
using Soluvion.Services;
using Soluvion.ViewModels;
using Soluvion.Views;

namespace Soluvion;

public static class MauiProgram
{
    public static string ConnectionString { get; } = "Server=Nitro;Database=Soluvion;User Id=tsvandra;Password=.tomAsk08.;TrustServerCertificate=True";

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

        // Regisztráljuk a szolgáltatásokat
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddTransient<LoginViewModel>();
        //builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<Views.Admin.AdminDashboardPage>();
        builder.Services.AddTransient<Views.SalonEmployee.SalonDashboardPage>();
        builder.Services.AddTransient<Views.Customer.CustomerDashboardPage>();

        // Regisztráljuk a nézeteket
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
#if DEBUG
builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}


