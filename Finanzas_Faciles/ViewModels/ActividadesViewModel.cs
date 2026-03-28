using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Finanzas_Faciles.Helpers;
using Finanzas_Faciles.Models;
using Finanzas_Faciles.Services;

namespace Finanzas_Faciles.ViewModels
{
    public class ActividadesViewModel : BaseViewModel
    {
        private readonly IActividadService _actividadService;
        private string _nombre = string.Empty;
        private string _costoDirectoTexto = string.Empty;
        private string _margenTexto = string.Empty;
        private string _precioFijoTexto = string.Empty;
        private ModoPrecioActividad _modoPrecio = ModoPrecioActividad.PorMargen;
        private EstadoActividad _estadoSeleccionado = EstadoActividad.Activa;
        private string _mensajeError = string.Empty;
        private bool _tieneError;
        private bool _mostrarFormulario;
        private int _actividadEnEdicionId;

        public ActividadesViewModel(IActividadService actividadService)
        {
            _actividadService = actividadService;
            Actividades = new ObservableCollection<Actividad>();
            GuardarCommand = new AsyncRelayCommand(GuardarAsync);
            NuevoCommand = new RelayCommand(AbrirFormularioNuevo);
            CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
            EditarCommand = new RelayCommand<Actividad>(Editar);
            EliminarCommand = new AsyncRelayCommand<Actividad>(EliminarAsync);
        }

        public ObservableCollection<Actividad> Actividades { get; }
        public Array Estados => Enum.GetValues(typeof(EstadoActividad));
        public Array ModosPrecio => Enum.GetValues(typeof(ModoPrecioActividad));

        public string Nombre { get => _nombre; set { if (SetProperty(ref _nombre, value)) LimpiarError(); } }
        public string CostoDirectoTexto { get => _costoDirectoTexto; set { if (SetProperty(ref _costoDirectoTexto, value)) LimpiarError(); } }
        public string MargenTexto { get => _margenTexto; set { if (SetProperty(ref _margenTexto, value)) LimpiarError(); } }
        public string PrecioFijoTexto { get => _precioFijoTexto; set { if (SetProperty(ref _precioFijoTexto, value)) LimpiarError(); } }
        
        public ModoPrecioActividad ModoPrecio
        {
            get => _modoPrecio;
            set
            {
                if (SetProperty(ref _modoPrecio, value))
                {
                    LimpiarError();
                    OnPropertyChanged(nameof(UsarMargen));
                    OnPropertyChanged(nameof(UsarPrecioFijo));
                }
            }
        }

        public bool UsarMargen => ModoPrecio == ModoPrecioActividad.PorMargen;
        public bool UsarPrecioFijo => ModoPrecio == ModoPrecioActividad.PrecioFijo;

        public EstadoActividad EstadoSeleccionado { get => _estadoSeleccionado; set => SetProperty(ref _estadoSeleccionado, value); }
        public string MensajeError { get => _mensajeError; set { SetProperty(ref _mensajeError, value); TieneError = !string.IsNullOrEmpty(value); } }
        public bool TieneError { get => _tieneError; set => SetProperty(ref _tieneError, value); }
        public bool MostrarFormulario { get => _mostrarFormulario; set => SetProperty(ref _mostrarFormulario, value); }
        public bool EsEdicion => _actividadEnEdicionId > 0;
        public string TituloFormulario => EsEdicion ? "Editar actividad" : "Alta de actividad";

        public ICommand GuardarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand CerrarFormularioCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public async Task CargarDatosAsync()
        {
            var items = await _actividadService.ObtenerTodasAsync();
            Actividades.Clear();
            foreach (var a in items)
                Actividades.Add(a);
        }


        private async Task GuardarAsync()
        {
            LimpiarError();

            if (string.IsNullOrWhiteSpace(Nombre)) { MensajeError = "Debe ingresar el nombre de la actividad."; return; }
            if (string.IsNullOrWhiteSpace(CostoDirectoTexto)) { MensajeError = "Debe ingresar el costo directo."; return; }
            if (!decimal.TryParse(CostoDirectoTexto.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var costo) || costo <= 0)
            { MensajeError = "El costo directo debe ser mayor a cero."; return; }

            decimal margen = 0;
            decimal? precioFijo = null;

            if (ModoPrecio == ModoPrecioActividad.PorMargen)
            {
                if (string.IsNullOrWhiteSpace(MargenTexto)) { MensajeError = "Debe ingresar el margen de ganancia."; return; }
                if (!decimal.TryParse(MargenTexto.Replace(",", "."), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out margen) || margen < 0)
                { MensajeError = "El margen no puede ser negativo."; return; }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(PrecioFijoTexto)) { MensajeError = "Debe ingresar el precio de venta."; return; }
                if (!decimal.TryParse(PrecioFijoTexto.Replace(",", "."), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                { MensajeError = "El precio de venta debe ser mayor a cero."; return; }
                if (precio < costo)
                { MensajeError = "El precio de venta no puede ser menor al costo directo."; return; }
                precioFijo = precio;
                margen = ((precio - costo) / costo) * 100;
            }

            try
            {
                if (EsEdicion)
                {
                    var actividad = await _actividadService.ObtenerPorIdAsync(_actividadEnEdicionId);
                    if (actividad == null) { MensajeError = "Actividad no encontrada."; return; }
                    actividad.Nombre = Nombre.Trim();
                    actividad.CostoDirecto = costo;
                    actividad.ModoPrecio = ModoPrecio;
                    actividad.MargenGanancia = margen;
                    actividad.PrecioVentaFijo = precioFijo;
                    actividad.Estado = EstadoSeleccionado;
                    await _actividadService.ActualizarAsync(actividad);
                    var idx = Actividades.ToList().FindIndex(a => a.Id == actividad.Id);
                    if (idx >= 0) Actividades[idx] = CrearCopia(actividad);
                }
                else
                {
                    var actividad = new Actividad
                    {
                        Nombre = Nombre.Trim(),
                        CostoDirecto = costo,
                        ModoPrecio = ModoPrecio,
                        MargenGanancia = margen,
                        PrecioVentaFijo = precioFijo,
                        Estado = EstadoSeleccionado
                    };
                    await _actividadService.AgregarAsync(actividad);
                    Actividades.Add(actividad);
                }
                LimpiarFormulario();
                CerrarFormulario();
            }
            catch (ValidationException ex) { MensajeError = ex.Message; }
        }


    }
}
