using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Models;
using FinanzasFaciles.Services;

namespace FinanzasFaciles.ViewModels;

public class HistorialRetirosViewModel : BaseViewModel
{
    private readonly IRetiroService _retiroService;
    private readonly IIngresoService _ingresoService;
    private PeriodoFiltro _periodoSeleccionado = PeriodoFiltro.Mes;
    private decimal _totalRetiradoEnPeriodo;
    private decimal _utilidadRealEnPeriodo;
    private string _mensajeConfirmacion = string.Empty;
    private int _retiroEnEdicionId;
    private string _montoTexto = string.Empty;
    private DateTime _fecha = DateTime.Today;
    private string _concepto = string.Empty;
    private string _mensajeError = string.Empty;
    private bool _tieneError;
    private string _mensajeExito = string.Empty;
    private bool _mostrarFormulario;

    public HistorialRetirosViewModel(IRetiroService retiroService, IIngresoService ingresoService)
    {
        _retiroService = retiroService;
        _ingresoService = ingresoService;
        Retiros = new ObservableCollection<Retiro>();
        PeriodosDisponibles = Enum.GetValues<PeriodoFiltro>();
        FiltrarCommand = new AsyncRelayCommand(FiltrarAsync);
        EditarCommand = new AsyncRelayCommand<Retiro>(IniciarEdicionAsync);
        EliminarCommand = new AsyncRelayCommand<Retiro>(EliminarAsync);
        GuardarCommand = new AsyncRelayCommand(GuardarEdicionAsync);
        CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
        _retiroService.RetiroRegistrado += OnRetiroRegistrado;
    }

    private void OnRetiroRegistrado(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => _ = FiltrarAsync());

    public ObservableCollection<Retiro> Retiros { get; }
    public PeriodoFiltro[] PeriodosDisponibles { get; }

    public string TituloFormulario => "Editar retiro";

    public bool MostrarFormulario
    {
        get => _mostrarFormulario;
        set => SetProperty(ref _mostrarFormulario, value);
    }

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

    public bool TieneError
    {
        get => _tieneError;
        set => SetProperty(ref _tieneError, value);
    }

    public string MensajeExito
    {
        get => _mensajeExito;
        set
        {
            if (SetProperty(ref _mensajeExito, value))
                OnPropertyChanged(nameof(MostrarMensajeExito));
        }
    }

    public bool MostrarMensajeExito => !string.IsNullOrEmpty(_mensajeExito);

    public PeriodoFiltro PeriodoSeleccionado
    {
        get => _periodoSeleccionado;
        set
        {
            if (SetProperty(ref _periodoSeleccionado, value))
                _ = FiltrarAsync();
        }
    }

    public decimal TotalRetiradoEnPeriodo
    {
        get => _totalRetiradoEnPeriodo;
        set => SetProperty(ref _totalRetiradoEnPeriodo, value);
    }

    public decimal UtilidadRealEnPeriodo
    {
        get => _utilidadRealEnPeriodo;
        set => SetProperty(ref _utilidadRealEnPeriodo, value);
    }

    public string MensajeConfirmacion
    {
        get => _mensajeConfirmacion;
        set => SetProperty(ref _mensajeConfirmacion, value);
    }

    public string ProporcionUtilidadRetirada
    {
        get
        {
            if (UtilidadRealEnPeriodo <= 0) return "N/A";
            var pct = (TotalRetiradoEnPeriodo / UtilidadRealEnPeriodo) * 100;
            return $"{pct:F1}% de la utilidad real retirada";
        }
    }

    public ICommand FiltrarCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand EliminarCommand { get; }
    public ICommand GuardarCommand { get; }
    public ICommand CerrarFormularioCommand { get; }

    public async Task CargarDatosAsync() => await FiltrarAsync();

    private Task IniciarEdicionAsync(Retiro? r)
    {
        if (r is null) return Task.CompletedTask;
        _retiroEnEdicionId = r.Id;
        MontoTexto = r.Monto.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Fecha = r.Fecha.Date;
        Concepto = r.Concepto;
        LimpiarError();
        MensajeExito = string.Empty;
        MostrarFormulario = true;
        return Task.CompletedTask;
    }

    private async Task GuardarEdicionAsync()
    {
        if (_retiroEnEdicionId == 0) return;
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

        var resultado = await _retiroService.ActualizarRetiroAsync(
            _retiroEnEdicionId, monto, Fecha, Concepto.Trim(), false);

        if (resultado.RequiereConfirmacionCapital)
        {
            var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
            var confirmado = page != null && await page.DisplayAlert(
                "Advertencia",
                resultado.MensajeError ?? "¿Continuar?",
                "Sí, continuar",
                "Cancelar");
            if (!confirmado) return;
            resultado = await _retiroService.ActualizarRetiroAsync(
                _retiroEnEdicionId, monto, Fecha, Concepto.Trim(), true);
        }

        if (!resultado.Exito)
        {
            MensajeError = resultado.MensajeError ?? "Error al actualizar el retiro.";
            return;
        }

        MostrarFormulario = false;
        _retiroEnEdicionId = 0;
        LimpiarFormulario();
        LimpiarError();
        MensajeExito = "Retiro actualizado correctamente.";
    }

    private async Task EliminarAsync(Retiro? r)
    {
        if (r is null) return;
        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
        if (page == null) return;
        var ok = await page.DisplayAlert("Eliminar retiro",
            $"¿Eliminar el retiro de {r.Concepto} por {r.Monto:C2} del {r.Fecha:dd/MM/yyyy}?",
            "Eliminar", "Cancelar");
        if (!ok) return;
        await _retiroService.EliminarAsync(r.Id);
    }

    private void CerrarFormulario()
    {
        MostrarFormulario = false;
        _retiroEnEdicionId = 0;
        LimpiarFormulario();
        LimpiarError();
    }

    private void LimpiarFormulario()
    {
        MontoTexto = string.Empty;
        Concepto = string.Empty;
        Fecha = DateTime.Today;
    }

    private void LimpiarError() => MensajeError = string.Empty;

    private async Task FiltrarAsync()
    {
        var (desde, hasta) = ObtenerRangoPeriodo();
        var retiros = await _retiroService.ObtenerPorPeriodoAsync(desde, hasta);
        var totalRetirado = await _retiroService.ObtenerTotalRetiradoEnPeriodoAsync(desde, hasta);
        var utilidadBruta = await _ingresoService.ObtenerUtilidadBrutaAcumuladaMensualAsync();

        Retiros.Clear();
        foreach (var r in retiros)
            Retiros.Add(r);

        TotalRetiradoEnPeriodo = totalRetirado;
        UtilidadRealEnPeriodo = utilidadBruta;
        OnPropertyChanged(nameof(ProporcionUtilidadRetirada));
    }

    private (DateTime desde, DateTime hasta) ObtenerRangoPeriodo()
    {
        var hoy = DateTime.Today;
        return PeriodoSeleccionado switch
        {
            PeriodoFiltro.Semana => (hoy.AddDays(-7), hoy),
            PeriodoFiltro.Mes => (new DateTime(hoy.Year, hoy.Month, 1), hoy),
            PeriodoFiltro.Trimestre => (hoy.AddMonths(-3), hoy),
            _ => (new DateTime(2020, 1, 1), hoy.AddDays(1))
        };
    }
}
