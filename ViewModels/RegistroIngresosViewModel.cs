using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Models;
using FinanzasFaciles.Services;

namespace FinanzasFaciles.ViewModels;

public class RegistroIngresosViewModel : BaseViewModel
{
    private readonly IIngresoService _ingresoService;
    private readonly IActividadService _actividadService;
    private Actividad? _actividadSeleccionada;
    private string _cantidadTexto = string.Empty;
    private DateTime _fecha = DateTime.Today;
    private string _mensajeError = string.Empty;
    private bool _tieneError;
    private bool _mostrarFormulario;
    private int _ingresoEnEdicionId;

    public RegistroIngresosViewModel(IIngresoService ingresoService, IActividadService actividadService)
    {
        _ingresoService = ingresoService;
        _actividadService = actividadService;
        Actividades = new ObservableCollection<Actividad>();
        IngresosRecientes = new ObservableCollection<Ingreso>();
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        NuevoCommand = new RelayCommand(AbrirFormularioNuevo);
        CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
        EditarCommand = new AsyncRelayCommand<Ingreso>(IniciarEdicionAsync);
        EliminarCommand = new AsyncRelayCommand<Ingreso>(EliminarAsync);
    }

    public ObservableCollection<Actividad> Actividades { get; }
    public ObservableCollection<Ingreso> IngresosRecientes { get; }

    public bool EsEdicion => _ingresoEnEdicionId > 0;

    public string TituloFormulario => EsEdicion ? "Editar ingreso" : "Registrar ingreso";

    public Actividad? ActividadSeleccionada
    {
        get => _actividadSeleccionada;
        set
        {
            if (SetProperty(ref _actividadSeleccionada, value))
            {
                LimpiarError();
                OnPropertyChanged(nameof(PickerActividadTitle));
            }
        }
    }

    public string PickerActividadTitle => _actividadSeleccionada is null ? "Seleccionar actividad" : string.Empty;

    public string CantidadTexto
    {
        get => _cantidadTexto;
        set
        {
            if (SetProperty(ref _cantidadTexto, value))
                LimpiarError();
        }
    }

    public DateTime Fecha
    {
        get => _fecha;
        set => SetProperty(ref _fecha, value);
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

    public bool MostrarFormulario
    {
        get => _mostrarFormulario;
        set => SetProperty(ref _mostrarFormulario, value);
    }

    public ICommand GuardarCommand { get; }
    public ICommand NuevoCommand { get; }
    public ICommand CerrarFormularioCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand EliminarCommand { get; }

    public async Task CargarDatosAsync()
    {
        var actividades = await _actividadService.ObtenerTodasAsync();
        Actividades.Clear();
        foreach (var a in actividades.Where(x => x.Estado == EstadoActividad.Activa))
            Actividades.Add(a);

        var ingresos = await _ingresoService.ObtenerIngresosDelPeriodoAsync();
        IngresosRecientes.Clear();
        foreach (var i in ingresos)
            IngresosRecientes.Add(i);
    }

    private async Task GuardarAsync()
    {
        LimpiarError();

        if (ActividadSeleccionada == null)
        {
            MensajeError = "Debe seleccionar una actividad del catálogo.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CantidadTexto) || !int.TryParse(CantidadTexto, out var cantidad) || cantidad <= 0)
        {
            MensajeError = "La cantidad debe ser un número entero mayor a cero.";
            return;
        }

        try
        {
            if (EsEdicion)
            {
                await _ingresoService.ActualizarAsync(new Ingreso
                {
                    Id = _ingresoEnEdicionId,
                    ActividadId = ActividadSeleccionada.Id,
                    Cantidad = cantidad,
                    Fecha = Fecha
                });
            }
            else
            {
                await _ingresoService.RegistrarAsync(new Ingreso
                {
                    ActividadId = ActividadSeleccionada.Id,
                    Cantidad = cantidad,
                    Fecha = Fecha
                });
            }

            await CargarDatosAsync();
            LimpiarFormulario();
            CerrarFormulario();
        }
        catch (ValidationException ex)
        {
            MensajeError = ex.Message;
        }
    }

    private async Task IniciarEdicionAsync(Ingreso? ingreso)
    {
        if (ingreso == null) return;
        await EnsureActividadEnListaAsync(ingreso.ActividadId);
        _ingresoEnEdicionId = ingreso.Id;
        ActividadSeleccionada = Actividades.FirstOrDefault(a => a.Id == ingreso.ActividadId);
        CantidadTexto = ingreso.Cantidad.ToString();
        Fecha = ingreso.Fecha.Date;
        LimpiarError();
        OnPropertyChanged(nameof(EsEdicion));
        OnPropertyChanged(nameof(TituloFormulario));
        MostrarFormulario = true;
    }

    private async Task EnsureActividadEnListaAsync(int actividadId)
    {
        if (Actividades.Any(a => a.Id == actividadId)) return;
        var a = await _actividadService.ObtenerPorIdAsync(actividadId);
        if (a != null)
            Actividades.Insert(0, a);
    }

    private async Task EliminarAsync(Ingreso? ingreso)
    {
        if (ingreso == null) return;
        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
        if (page == null) return;
        var ok = await page.DisplayAlert("Eliminar ingreso",
            $"¿Quitar el ingreso de {ingreso.NombreActividad} del {ingreso.Fecha:dd/MM/yyyy}?",
            "Eliminar", "Cancelar");
        if (!ok) return;
        await _ingresoService.EliminarAsync(ingreso.Id);
        var quitar = IngresosRecientes.FirstOrDefault(x => x.Id == ingreso.Id);
        if (quitar != null)
            IngresosRecientes.Remove(quitar);
    }

    private void AbrirFormularioNuevo()
    {
        _ingresoEnEdicionId = 0;
        LimpiarFormulario();
        LimpiarError();
        Fecha = DateTime.Today;
        OnPropertyChanged(nameof(EsEdicion));
        OnPropertyChanged(nameof(TituloFormulario));
        MostrarFormulario = true;
    }

    private void CerrarFormulario()
    {
        MostrarFormulario = false;
        _ingresoEnEdicionId = 0;
        LimpiarFormulario();
        LimpiarError();
        OnPropertyChanged(nameof(EsEdicion));
        OnPropertyChanged(nameof(TituloFormulario));
    }

    private void LimpiarFormulario()
    {
        ActividadSeleccionada = null;
        CantidadTexto = string.Empty;
        Fecha = DateTime.Today;
    }

    private void LimpiarError()
    {
        MensajeError = string.Empty;
    }
}
