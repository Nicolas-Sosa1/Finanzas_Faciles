namespace FinanzasFaciles;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		var temaGuardado = Preferences.Get("AppUserTheme", string.Empty);
		if (Enum.TryParse<AppTheme>(temaGuardado, out var tema))
			UserAppTheme = tema;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}