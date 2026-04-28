using FinanzasFaciles.ViewModels;

namespace FinanzasFaciles.Views;

public partial class ExportarPage : ContentPage
{
    public ExportarPage()
    {
        InitializeComponent();

        BindingContext = Application.Current
            .Handler
            .MauiContext
            .Services
            .GetService<ExportViewModel>();
    }
}