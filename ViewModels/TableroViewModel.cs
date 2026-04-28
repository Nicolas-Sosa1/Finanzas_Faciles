using System.Collections.ObjectModel;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Models;
using FinanzasFaciles.Services;

namespace FinanzasFaciles.ViewModels;

public class TableroViewModel : BaseViewModel
{
    private readonly IGastoFijoService _gastoFijoService;
    private readonly IIngresoService _ingresoService;
    private readonly IRetiroService _retiroService;
    private decimal _totalCostosFijos;
    private decimal _utilidadBrutaAcumulada;
    private decimal _totalRetiros;
    private decimal _utilidadReal;
    private decimal _costosDirectosAcumulados;
    private decimal _saldoCaja;
    private decimal _efectivoDisponible;
    private decimal _excedenteOFaltante;
    private bool _tieneExcedente;
    private double _progresoCobertura;
    private string _mensajeAlerta = string.Empty;
    private bool _tieneAlerta;
    private bool _sinCostosFijos;

    private readonly IActividadService _actividadService;

    public TableroViewModel(IGastoFijoService gastoFijoService, IIngresoService ingresoService,
        IRetiroService retiroService, IActividadService actividadService)
    {
        _gastoFijoService = gastoFijoService;
        _ingresoService = ingresoService;
        _retiroService = retiroService;
        _actividadService = actividadService;

        IngresosRecientes = new ObservableCollection<Ingreso>();

        void Refrescar() => MainThread.BeginInvokeOnMainThread(() => _ = ActualizarAsync());
        _ingresoService.IngresoRegistrado += (_, _) => Refrescar();
        _retiroService.RetiroRegistrado += (_, _) => Refrescar();
        _gastoFijoService.DatoActualizado += (_, _) => Refrescar();
        _actividadService.DatoActualizado += (_, _) => Refrescar();
    }

    public ObservableCollection<Ingreso> IngresosRecientes { get; }

        public decimal TotalCostosFijos
    {
        get => _totalCostosFijos;
        set => SetProperty(ref _totalCostosFijos, value);
    }

        public decimal UtilidadBrutaAcumulada
    {
        get => _utilidadBrutaAcumulada;
        set => SetProperty(ref _utilidadBrutaAcumulada, value);
    }

        public decimal TotalRetiros
    {
        get => _totalRetiros;
        set => SetProperty(ref _totalRetiros, value);
    }

        public decimal UtilidadReal
    {
        get => _utilidadReal;
        set => SetProperty(ref _utilidadReal, value);
    }

        public double ProgresoCobertura
    {
        get => _progresoCobertura;
        set => SetProperty(ref _progresoCobertura, value);
    }

        public string EstadoFinanciero =>
        SinCostosFijos ? "Sin configurar" :
        TieneExcedente ? "Ganancia Neta Disponible" : "Fase de Cobertura";

        public string MensajeEstado =>
        SinCostosFijos ? MensajeAlerta :
        TieneExcedente ? $"Superávit disponible: {ExcedenteOFaltante:C2}" :
        $"Monto pendiente para equilibrio: {Math.Abs(ExcedenteOFaltante):C2}";

        public decimal CostosDirectosAcumulados
    {
        get => _costosDirectosAcumulados;
        set => SetProperty(ref _costosDirectosAcumulados, value);
    }

        public decimal SaldoCaja
    {
        get => _saldoCaja;
        set => SetProperty(ref _saldoCaja, value);
    }

        public decimal EfectivoDisponible
    {
        get => _efectivoDisponible;
        set => SetProperty(ref _efectivoDisponible, value);
    }

        public decimal ExcedenteOFaltante
    {
        get => _excedenteOFaltante;
        set => SetProperty(ref _excedenteOFaltante, value);
    }

        public bool TieneExcedente
    {
        get => _tieneExcedente;
        set => SetProperty(ref _tieneExcedente, value);
    }

    public string TextoPuntoEquilibrio
    {
        get => TieneExcedente
            ? $"Superávit: {ExcedenteOFaltante:C2}"
            : ExcedenteOFaltante < 0
                ? $"Falta: {Math.Abs(ExcedenteOFaltante):C2}"
                : "En punto de equilibrio";
    }

    public string MensajeAlerta
    {
        get => _mensajeAlerta;
        set
        {
            SetProperty(ref _mensajeAlerta, value);
            TieneAlerta = !string.IsNullOrEmpty(value);
        }
    }

    public bool TieneAlerta
    {
        get => _tieneAlerta;
        set => SetProperty(ref _tieneAlerta, value);
    }

        public bool SinCostosFijos
    {
        get => _sinCostosFijos;
        set => SetProperty(ref _sinCostosFijos, value);
    }

        public bool MostrarAlertaAmarilla => TieneAlerta && !SinCostosFijos;

    public async Task CargarDatosAsync()
    {
        await ActualizarAsync();
    }

    private async Task ActualizarAsync()
    {
        TotalCostosFijos = await _gastoFijoService.ObtenerTotalCostosFijosMensualAsync();
        UtilidadBrutaAcumulada = await _ingresoService.ObtenerUtilidadBrutaAcumuladaMensualAsync();
        TotalRetiros = await _retiroService.ObtenerTotalRetiradoAsync();
        UtilidadReal = UtilidadBrutaAcumulada - TotalRetiros;
        CostosDirectosAcumulados = await _ingresoService.ObtenerCostosDirectosAcumuladosMensualAsync();
        SaldoCaja = await _ingresoService.ObtenerSaldoCajaAsync();
        EfectivoDisponible = SaldoCaja - TotalRetiros;

        CalcularPuntoDeEquilibrio();
        EvaluarAlertas();
        await CargarIngresosRecientesAsync();

        ActualizarProgresoCobertura();

        OnPropertyChanged(nameof(TextoPuntoEquilibrio));
        OnPropertyChanged(nameof(EstadoFinanciero));
        OnPropertyChanged(nameof(MensajeEstado));
        OnPropertyChanged(nameof(MostrarAlertaAmarilla));
    }

        private void CalcularPuntoDeEquilibrio()
    {
        ExcedenteOFaltante = UtilidadReal - TotalCostosFijos;
        TieneExcedente = ExcedenteOFaltante > 0;
    }

        private void ActualizarProgresoCobertura()
    {
        ProgresoCobertura = TotalCostosFijos > 0
            ? Math.Clamp((double)(UtilidadReal / TotalCostosFijos), 0, 1.0)
            : 0;
    }

        private void EvaluarAlertas()
    {
        SinCostosFijos = TotalCostosFijos == 0;

        if (SinCostosFijos)
        {
            MensajeAlerta = "Completá la configuración de costos fijos para habilitar el monitor de salud financiera.";
            return;
        }

        
        if (EfectivoDisponible < CostosDirectosAcumulados && CostosDirectosAcumulados > 0)
        {
            MensajeAlerta = "Atención: El efectivo disponible podría ser insuficiente para sostener el capital de operación.";
            return;
        }

        if (!TieneExcedente && TotalCostosFijos > 0)
        {
            MensajeAlerta = $"La utilidad bruta del período aún no cubre los costos fijos. Falta {Math.Abs(ExcedenteOFaltante):C2}.";
            return;
        }

        MensajeAlerta = string.Empty;
    }

    private async Task CargarIngresosRecientesAsync()
    {
        var ingresos = await _ingresoService.ObtenerIngresosDelPeriodoAsync();
        IngresosRecientes.Clear();
        foreach (var i in ingresos.Take(10))
            IngresosRecientes.Add(i);
    }
}
