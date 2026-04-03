using Finanzas_Faciles.ViewModels;
namespace Finanzas_Faciles.Views;


public partial class RegistroIngresosPage : ContentPage
{
    public RegistroIngresosPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<IngresosViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is IngresosViewModel vm)
                await vm.CargarDatosAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is IngresosViewModel vm)
            await vm.CargarDatosAsync();
    }
}
