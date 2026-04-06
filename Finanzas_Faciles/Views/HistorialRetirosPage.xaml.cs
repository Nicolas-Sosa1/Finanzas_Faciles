using Finanzas_Faciles.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Finanzas_Faciles.Views;

public partial class HistorialRetirosPage : ContentPage
{
    public HistorialRetirosPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<HistorialRetirosViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is HistorialRetirosViewModel vm)
                await vm.CargarDatosAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistorialRetirosViewModel vm)
            await vm.CargarDatosAsync();
    }
}
