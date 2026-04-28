using System.Windows.Input;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Services;

namespace FinanzasFaciles.ViewModels;

public class RetirosViewModel : BaseViewModel
{
    private readonly IRetiroService _retiroService;
    private string _montoTexto = string.Empty;
    private DateTime _fecha = DateTime.Today;
    private string _concepto = string.Empty;
    private string _mensajeError = string.Empty;
    private bool _tieneError;
    private string _mensajeExito = string.Empty;
    private bool _mostrarFormulario;
    private decimal _efectivoDisponible;

    public RetirosViewModel(IRetiroService retiroService)
    {
        _retiroService = retiroService;
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        NuevoCommand = new RelayCommand(AbrirFormulario);
        CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
        _retiroService.RetiroRegistrado += OnRetiroRegistrado;
    }

    private void OnRetiroRegistrado(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => _ = CargarDatosAsync());

    public string MontoTexto
    {
        get => _montoTexto;
        set
        {
            if (SetProperty(ref _montoTexto, value))
                LimpiarError();
        }
    }

    public DateTime Fecha
    {
        get => _fecha;
        set => SetProperty(ref _fecha, value);
    }

    public string Concepto
    {
        get => _concepto;
        set
        {
            if (SetProperty(ref _concepto, value))
                LimpiarError();
        }
    }

    public string MensajeError
    {
        get => _mensajeError;
        set
        {
            SetProperty(ref _mensajeError, value);
            TieneError = !string.IsNullOrEmpty(value);
        }
    }

    public string MensajeExito
    {
        get => _mensajeExito;
        set => SetProperty(ref _mensajeExito, value);
    }

    public bool TieneError
    {
        get => _tieneError;
        set => SetProperty(ref _tieneError, value);
    }

    public bool MostrarFormulario
    {
        get => _mostrarFormulario;
        set => SetProperty(ref _mostrarFormulario, value);
    }

    public decimal EfectivoDisponible
    {
        get => _efectivoDisponible;
        set => SetProperty(ref _efectivoDisponible, value);
    }

    public ICommand GuardarCommand { get; }
    public ICommand NuevoCommand { get; }
    public ICommand CerrarFormularioCommand { get; }

    public async Task CargarDatosAsync()
    {
        var estado = await _retiroService.ObtenerEstadoFinancieroAsync();
        EfectivoDisponible = estado.EfectivoDisponible;
    }

    private async Task GuardarAsync()
    {
        LimpiarError();
        MensajeExito = string.Empty;

        if (string.IsNullOrWhiteSpace(MontoTexto) ||
            !decimal.TryParse(MontoTexto.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var monto) || monto <= 0)
        {
            MensajeError = "El monto debe ser un valor mayor a cero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Concepto))
        {
            MensajeError = "Debe ingresar un concepto o descripción.";
            return;
        }

        var resultado = await _retiroService.RegistrarRetiroAsync(monto, Fecha, Concepto.Trim(), false);

        if (resultado.RequiereConfirmacionCapital)
        {
            var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
            var confirmado = page != null && await page.DisplayAlert(
                "Advertencia",
                resultado.MensajeError ?? "Este retiro comprometería el capital de operación. ¿Continuar?",
                "Sí, continuar",
                "Cancelar");

            if (!confirmado)
                return;

            resultado = await _retiroService.RegistrarRetiroAsync(monto, Fecha, Concepto.Trim(), true);
        }

        if (!resultado.Exito)
        {
            MensajeError = resultado.MensajeError ?? "Error al registrar el retiro.";
            return;
        }

        MensajeExito = $"Retiro registrado: {resultado.Retiro!.TipoRetiro}. Monto: {resultado.Retiro.Monto:C2}.";
        await CargarDatosAsync();
        LimpiarFormulario();
        CerrarFormulario();
    }

    private void AbrirFormulario()
    {
        LimpiarFormulario();
        LimpiarError();
        MensajeExito = string.Empty;
        Fecha = DateTime.Today;
        MostrarFormulario = true;
        _ = CargarDatosAsync();
    }

    private void CerrarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
        LimpiarError();
    }

    private void LimpiarFormulario()
    {
        MontoTexto = string.Empty;
        Concepto = string.Empty;
        Fecha = DateTime.Today;
    }

    private void LimpiarError()
    {
        MensajeError = string.Empty;
    }
}
