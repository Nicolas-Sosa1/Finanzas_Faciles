using FinanzasFaciles.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FinanzasFaciles.Views;

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

        var viewModel = services.GetRequiredService<GastoFijosViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is GastoFijosViewModel vm)
                await vm.CargarDatosAsync();
        };
    }
}
