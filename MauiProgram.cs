using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FinanzasFaciles.Services;
using FinanzasFaciles.ViewModels;
using System.Globalization;
using FinanzasFaciles.Views;

namespace FinanzasFaciles;

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

		
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "FinanzasFaciles.db3");
		builder.Services.AddDbContextFactory<AppDbContext>(options =>
			options.UseSqlite($"Filename={dbPath}"));

		
		builder.Services.AddSingleton<DatabaseService>();

		
		builder.Services.AddSingleton<IGastoFijoService, SqliteGastoFijoService>();
		builder.Services.AddSingleton<IActividadService, SqliteActividadService>();
		builder.Services.AddSingleton<IIngresoService, SqliteIngresoService>();
		builder.Services.AddSingleton<IRetiroService, SqliteRetiroService>();
        builder.Services.AddTransient<IExportService, ExportService>();
        
        builder.Services.AddTransient<GastoFijosViewModel>();
		builder.Services.AddTransient<ActividadesViewModel>();
		builder.Services.AddTransient<TableroViewModel>();
		builder.Services.AddTransient<RegistroIngresosViewModel>();
		builder.Services.AddTransient<RetirosViewModel>();
		builder.Services.AddTransient<HistorialRetirosViewModel>();
        builder.Services.AddTransient<ExportViewModel>();
        builder.Services.AddTransient<ExportarPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        
        var culture = new CultureInfo("es-AR"); 
        culture.NumberFormat.CurrencySymbol = "$"; 
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var app = builder.Build();

		
		var dbService = app.Services.GetRequiredService<DatabaseService>();
		dbService.InitializeAsync().GetAwaiter().GetResult();

		return app;
	}
}
