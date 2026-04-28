using FinanzasFaciles.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FinanzasFaciles.Views;

public partial class ActividadesPage : ContentPage
{
    public ActividadesPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<ActividadesViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is ActividadesViewModel vm)
                await vm.CargarDatosAsync();
        };
    }
}
