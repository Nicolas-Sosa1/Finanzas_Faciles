using Finanzas_Faciles.ViewModels;

namespace Finanzas_Faciles.Views;

public partial class GastoFijosPage : ContentPage
{
    public GastoFijosPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<GastoFijoViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is GastoFijoViewModel vm)
                await vm.CargarDatosAsync();
        };
    }
}