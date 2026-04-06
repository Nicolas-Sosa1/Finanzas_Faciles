using Finanzas_Faciles.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Finanzas_Faciles.ViewModels;

namespace Finanzas_Faciles
{
    public static class MauiProgram
    {
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

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "proyectoreferencia.db3");
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite($"Filename={dbPath}"));

            builder.Services.AddSingleton<DatabaseService>();
            
            builder.Services.AddSingleton<IGastoFijoService, SqliteGastoFijoService>();
            builder.Services.AddSingleton<IIngresoService, SqliteIngresoService>();
            builder.Services.AddSingleton<IActividadService, SqliteActividadService>();
            builder.Services.AddSingleton<IRetiroService, SqliteRetiroService>();

            builder.Services.AddTransient<GastoFijoViewModel>();
            builder.Services.AddTransient<IngresosViewModel>();
            builder.Services.AddTransient<ActividadesViewModel>(); 
            builder.Services.AddTransient<RetirosViewModel>();
            builder.Services.AddTransient<HistorialRetirosViewModel>();

#if DEBUG
            builder.Logging.AddDebug();

#endif
            var app = builder.Build();

            // Crear la base de datos si no existe (persistencia en FileSystem.AppDataDirectory)
            var dbService = app.Services.GetRequiredService<DatabaseService>();
            dbService.InitializeAsync().GetAwaiter().GetResult();

            return app;
        }
    }
}
