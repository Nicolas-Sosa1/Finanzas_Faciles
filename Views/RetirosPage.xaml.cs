using FinanzasFaciles.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FinanzasFaciles.Views;

public partial class RetirosPage : ContentPage
{
    public RetirosPage()
    {
        InitializeComponent();
        CargarViewModel();
    }

    private void CargarViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Contenedor de servicios no disponible.");

        var viewModel = services.GetRequiredService<RetirosViewModel>();
        BindingContext = viewModel;

        Loaded += async (_, _) =>
        {
            if (BindingContext is RetirosViewModel vm)
                await vm.CargarDatosAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RetirosViewModel vm)
            await vm.CargarDatosAsync();
    }
}
