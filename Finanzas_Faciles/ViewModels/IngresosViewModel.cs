using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Finanzas_Faciles.Helpers;
using Finanzas_Faciles.Models;
using Finanzas_Faciles.Services;


namespace Finanzas_Faciles.ViewModels
{
    public class IngresosViewModel : BaseViewModel
    {
        private readonly IIngresoService _ingresoService;
        private readonly IActividadService _actividadService;
        private Actividad? _actividadSeleccionada;
        private string _cantidadTexto = string.Empty;
        private DateTime _fecha = DateTime.Today;
        private string _mensajeError = string.Empty;
        private bool _tieneError;
        private bool _mostrarFormulario;

        public IngresosViewModel(IIngresoService ingresoService, IActividadService actividadService)
        {
            _ingresoService = ingresoService;
            _actividadService = actividadService;
            Actividades = new ObservableCollection<Actividad>();
            IngresosRecientes = new ObservableCollection<Ingreso>();
            GuardarCommand = new AsyncRelayCommand(GuardarAsync);
            NuevoCommand = new RelayCommand(AbrirFormulario);
            CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
        }

        public ObservableCollection<Actividad> Actividades { get; }
        public ObservableCollection<Ingreso> IngresosRecientes { get; }

        public Actividad? ActividadSeleccionada
        {
            get => _actividadSeleccionada;
            set
            {
                if (SetProperty(ref _actividadSeleccionada, value))
                    LimpiarError();
            }
        }

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

    }
}
