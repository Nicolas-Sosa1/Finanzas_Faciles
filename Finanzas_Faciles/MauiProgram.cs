using Finanzas_Faciles.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

#if DEBUG
            builder.Logging.AddDebug();

            var app = builder.Build();
#endif

            // Crear la base de datos si no existe (persistencia en FileSystem.AppDataDirectory)
            var dbService = app.Services.GetRequiredService<DatabaseService>();
            dbService.InitializeAsync().GetAwaiter().GetResult();

            return app;
        }
    }
}
