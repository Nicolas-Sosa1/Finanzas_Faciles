using System.Windows.Input;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Services;

namespace FinanzasFaciles.ViewModels;

public class ExportViewModel : BaseViewModel
{
    private readonly IExportService _exportService;
    private readonly IIngresoService _ingresoService;
    private readonly IRetiroService _retiroService;
    private readonly IGastoFijoService _gastoFijoService;

    private bool _isExporting;
    private string _mensajeEstado = string.Empty;
    private bool _tieneExito;
    private bool _tieneError;
    private string _ultimoArchivo = string.Empty;

    
    private int _anioSeleccionado = DateTime.Today.Year;
    private int _mesSeleccionado = DateTime.Today.Month;

    public ExportViewModel(
        IExportService exportService,
        IIngresoService ingresoService,
        IRetiroService retiroService,
        IGastoFijoService gastoFijoService)
    {
        _exportService = exportService;
        _ingresoService = ingresoService;
        _retiroService = retiroService;
        _gastoFijoService = gastoFijoService;

        ExportarPdfCommand = new AsyncRelayCommand(ExportarPdfAsync, () => !IsExporting);
        ExportarExcelCommand = new AsyncRelayCommand(ExportarExcelAsync, () => !IsExporting);
        CompartirUltimoArchivoCommand = new AsyncRelayCommand(CompartirAsync,
            () => !string.IsNullOrEmpty(_ultimoArchivo));

        
        MesesDisponibles = Enumerable.Range(0, 12)
            .Select(i => DateTime.Today.AddMonths(-i))
            .Select(d => new MesItem(d.Year, d.Month))
            .ToList();

        MesSeleccionadoItem = MesesDisponibles.First();
    }

    

    public List<MesItem> MesesDisponibles { get; }

    private MesItem _mesSeleccionadoItem = null!;
    public MesItem MesSeleccionadoItem
    {
        get => _mesSeleccionadoItem;
        set
        {
            if (SetProperty(ref _mesSeleccionadoItem, value) && value != null)
            {
                _anioSeleccionado = value.Anio;
                _mesSeleccionado = value.Mes;
                LimpiarEstado();
            }
        }
    }

    public bool IsExporting
    {
        get => _isExporting;
        set
        {
            SetProperty(ref _isExporting, value);
            ((AsyncRelayCommand)ExportarPdfCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)ExportarExcelCommand).RaiseCanExecuteChanged();
        }
    }

    public string MensajeEstado
    {
        get => _mensajeEstado;
        set => SetProperty(ref _mensajeEstado, value);
    }

    public bool TieneExito
    {
        get => _tieneExito;
        set => SetProperty(ref _tieneExito, value);
    }

    public bool TieneError
    {
        get => _tieneError;
        set => SetProperty(ref _tieneError, value);
    }

    public bool MostrarCompartir => !string.IsNullOrEmpty(_ultimoArchivo);

    

    public ICommand ExportarPdfCommand { get; }
    public ICommand ExportarExcelCommand { get; }
    public ICommand CompartirUltimoArchivoCommand { get; }

    

    private async Task ExportarPdfAsync()
    {
        await EjecutarExportacionAsync(async resumen =>
        {
            var path = await _exportService.ExportarResumenMensualPdfAsync(resumen);
            return path;
        }, "PDF");
    }

    private async Task ExportarExcelAsync()
    {
        await EjecutarExportacionAsync(async resumen =>
        {
            var path = await _exportService.ExportarResumenMensualExcelAsync(resumen);
            return path;
        }, "Excel");
    }

    private async Task EjecutarExportacionAsync(Func<ResumenMensualDto, Task<string>> exportar,
        string tipoArchivo)
    {
        IsExporting = true;
        LimpiarEstado();

        try
        {
            var resumen = await ConstruirResumenAsync();
            var filePath = await exportar(resumen);

            _ultimoArchivo = filePath;
            OnPropertyChanged(nameof(MostrarCompartir));
            ((AsyncRelayCommand)CompartirUltimoArchivoCommand).RaiseCanExecuteChanged();

            TieneExito = true;
            MensajeEstado = $"✔ {tipoArchivo} generado correctamente.";

            
#if WINDOWS
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
#else
            await CompartirAsync();
#endif
        }
        catch (Exception ex)
        {
            TieneError = true;
            MensajeEstado = $"Error al generar {tipoArchivo}: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private async Task CompartirAsync()
    {
        if (string.IsNullOrEmpty(_ultimoArchivo)) return;
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir reporte — Finanzas Fáciles",
            File = new ShareFile(_ultimoArchivo)
        });
    }

        private async Task<ResumenMensualDto> ConstruirResumenAsync()
    {
        var inicioMes = new DateTime(_anioSeleccionado, _mesSeleccionado, 1);
        var finMes = inicioMes.AddMonths(1);

        
        
        
        
        var ingresos = (await _ingresoService.ObtenerIngresosDelPeriodoAsync())
            .Where(i => i.Fecha >= inicioMes && i.Fecha < finMes)
            .ToList();

        var retiros = (await _retiroService.ObtenerPorPeriodoAsync(inicioMes, finMes.AddDays(-1)))
            .ToList();

        var gastosFijos = (await _gastoFijoService.ObtenerTodosAsync()).ToList();

        var totalRetiros = retiros.Sum(r => r.Monto);
        var utilidadBruta = ingresos.Sum(i => i.UtilidadBruta);
        var costosDirectos = ingresos.Sum(i => i.FondoOperacion);
        var saldoCaja = ingresos.Sum(i => i.MontoTotal);
        var totalCostosFijos = gastosFijos.Where(g => g.Activo).Sum(g => g.MontoMensual);

        return new ResumenMensualDto
        {
            Anio = _anioSeleccionado,
            Mes = _mesSeleccionado,
            TotalCostosFijos = totalCostosFijos,
            UtilidadBruta = utilidadBruta,
            TotalRetiros = totalRetiros,
            EfectivoDisponible = saldoCaja - totalRetiros,
            CostosDirectosAcumulados = costosDirectos,
            Ingresos = ingresos,
            Retiros = retiros,
            GastosFijos = gastosFijos
        };
    }

    private void LimpiarEstado()
    {
        MensajeEstado = string.Empty;
        TieneExito = false;
        TieneError = false;
    }
}

public record MesItem(int Anio, int Mes)
{
    public string Nombre => new DateTime(Anio, Mes, 1)
        .ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-AR"));
    public override string ToString() => Nombre;
}
