using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using FinanzasFaciles.Services;
using FinanzasFaciles.ViewModels;
using FinanzasFaciles.Views;

namespace FinanzasFaciles;

public partial class AppShell : Shell
{
	private const string ThemePrefKey = "AppUserTheme";
	private const string AppBarTitle = "Finanzas Fáciles";

	private static readonly Color BrandBarBlue = Color.FromArgb("FF263B53");
	private static readonly Color WinUINavBarOpenLight = Color.FromArgb("FFF5F7F8");

	public AppShell()
	{
		InitializeComponent();
		UpdateThemeIcon();
		ApplyShellChrome();

		Loaded += OnShellLoaded;
		Navigated += OnShellNavigated;
		HandlerChanged += OnShellHandlerChanged;
        Routing.RegisterRoute(nameof(ExportarPage), typeof(ExportarPage));


    }

        private void ApplyShellChrome()
	{
		Title = AppBarTitle;
		if (Application.Current is { } app)
		{
			if (app.Windows.Count > 0)
				app.Windows[0].Title = AppBarTitle;
		}
		if (CurrentPage is { } p)
			p.Title = AppBarTitle;

		if (IsWinUIMaui() && Application.Current is { })
			ApplyWinUINavColors();
	}

	private void OnShellLoaded(object? sender, EventArgs e)
	{
		ApplyShellChrome();
		if (IsWinUIMaui())
			ScheduleWinUINavReapply();
	}

	private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
	{
		ApplyShellChrome();
		if (IsWinUIMaui())
			ScheduleWinUINavReapply();
	}

	private void OnShellHandlerChanged(object? sender, EventArgs e)
	{
		ApplyShellChrome();
		if (IsWinUIMaui())
			ScheduleWinUINavReapply();
	}

	private void ScheduleWinUINavReapply()
	{
		MainThread.BeginInvokeOnMainThread(ApplyWinUINavColors);
		_ = Task.Run(async () =>
		{
			await Task.Delay(80).ConfigureAwait(true);
			MainThread.BeginInvokeOnMainThread(ApplyWinUINavColors);
		});
	}

	protected override void OnPropertyChanged(string? propertyName = null)
	{
		base.OnPropertyChanged(propertyName);
		if (propertyName == nameof(FlyoutIsPresented))
		{
			ApplyShellChrome();
			if (IsWinUIMaui())
				ApplyWinUINavColors();
		}
	}

	private async void OnRestablecerDatosClicked(object? sender, EventArgs e)
	{
		var ok = await DisplayAlert("Restablecer datos",
			"Se eliminan todos los gastos fijos, actividades, ingresos y retiros. Esta acción no se puede deshacer.",
			"Continuar", "Cancelar");
		if (!ok) return;
		DatabaseService? db;
		try
		{
			db = Application.Current?.Handler?.MauiContext?.Services.GetService<DatabaseService>();
		}
		catch
		{
			db = null;
		}
		if (db is null)
		{
			await DisplayAlert("Error", "No se pudo acceder a la base de datos.", "OK");
			return;
		}
		try
		{
			await db.BorrarTodosLosDatosAsync();
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
			return;
		}
		FlyoutIsPresented = false;
		await GoToAsync($"//{nameof(DashboardPage)}");
		if (CurrentPage is DashboardPage { BindingContext: TableroViewModel t })
			await t.CargarDatosAsync();
		await DisplayAlert("Listo", "Base vacía. Al entrar a cada pantalla verás listas sin datos.", "OK");
	}

	private void OnThemeToggleClicked(object? sender, EventArgs e)
	{
		if (Application.Current is null)
			return;

		var actual = Application.Current.UserAppTheme == AppTheme.Unspecified
			? Application.Current.RequestedTheme
			: Application.Current.UserAppTheme;

		var nuevo = actual == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
		Application.Current.UserAppTheme = nuevo;
		Preferences.Set(ThemePrefKey, nuevo.ToString());
		UpdateThemeIcon();
		ApplyShellChrome();
	}

	private void UpdateThemeIcon()
	{
		if (Application.Current is null)
			return;

		var actual = Application.Current.UserAppTheme == AppTheme.Unspecified
			? Application.Current.RequestedTheme
			: Application.Current.UserAppTheme;

		ThemeToggleButton.Text = actual == AppTheme.Dark ? "☀" : "🌙";
	}

		private void ApplyWinUINavColors()
	{
		if (Application.Current is null)
			return;

		var appTheme = Application.Current.UserAppTheme == AppTheme.Unspecified
			? Application.Current.RequestedTheme
			: Application.Current.UserAppTheme;

		var isDark = appTheme == AppTheme.Dark;
		if (FlyoutIsPresented && !isDark)
		{
			Shell.SetBackgroundColor(this, WinUINavBarOpenLight);
			Shell.SetTitleColor(this, BrandBarBlue);
			Shell.SetForegroundColor(this, BrandBarBlue);
			SetHamburgerIcon(BrandBarBlue);
		}
		else
		{
			Shell.SetBackgroundColor(this, BrandBarBlue);
			Shell.SetTitleColor(this, Colors.White);
			Shell.SetForegroundColor(this, Colors.White);
			SetHamburgerIcon(Colors.White);
		}
	}

	private void SetHamburgerIcon(Color color)
	{
		FlyoutIcon = new FontImageSource
		{
			Glyph = "\uE700",
			FontFamily = "Segoe MDL2 Assets",
			Size = 20,
			Color = color
		};
	}

	private static bool IsWinUIMaui()
	{
		var p = DeviceInfo.Current.Platform;
		return p == DevicePlatform.WinUI
			|| string.Equals(p.ToString(), "Win32", StringComparison.OrdinalIgnoreCase);
	}


}
