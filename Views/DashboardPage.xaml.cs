using FinanzasFaciles.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FinanzasFaciles.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<TableroViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is TableroViewModel vm)
                await vm.CargarDatosAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TableroViewModel vm)
            await vm.CargarDatosAsync();
    }
}
