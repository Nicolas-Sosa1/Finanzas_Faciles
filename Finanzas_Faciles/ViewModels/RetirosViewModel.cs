using System.Windows.Input;
using Finanzas_Faciles.Helpers;
using Finanzas_Faciles.Services;

namespace Finanzas_Faciles.ViewModels;

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

}
