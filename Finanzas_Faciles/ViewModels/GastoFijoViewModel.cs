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
    public class GastoFijoViewModel : BaseViewModel
    {
        private readonly IGastoFijoService _gastoFijoService;
        private string _nombre = string.Empty;
        private string _montoTexto = string.Empty;
        private CategoriaGastoFijo _categoriaSeleccionada = CategoriaGastoFijo.Otros;
        private bool _activo = true;
        private string _mensajeError = string.Empty;
        private bool _tieneError;
        private decimal _totalCostosFijos;
        private bool _mostrarFormulario;
        private int _gastoEnEdicionId; // 0 = nuevo

        public GastoFijosViewModel(IGastoFijoService gastoFijoService)
        {
            _gastoFijoService = gastoFijoService;
            Gastos = new ObservableCollection<GastoFijo>();
            GuardarCommand = new AsyncRelayCommand(GuardarAsync);
            NuevoCommand = new RelayCommand(AbrirFormularioNuevo);
            CerrarFormularioCommand = new RelayCommand(CerrarFormulario);
            EditarCommand = new RelayCommand<GastoFijo>(Editar);
            EliminarCommand = new AsyncRelayCommand<GastoFijo>(EliminarAsync);
        }

        public ObservableCollection<GastoFijo> Gastos { get; }
        public Array Categorias => Enum.GetValues(typeof(CategoriaGastoFijo));

        public string Nombre { get => _nombre; set { if (SetProperty(ref _nombre, value)) LimpiarError(); } }
        public string MontoTexto { get => _montoTexto; set { if (SetProperty(ref _montoTexto, value)) LimpiarError(); } }
        public CategoriaGastoFijo CategoriaSeleccionada { get => _categoriaSeleccionada; set => SetProperty(ref _categoriaSeleccionada, value); }
        public bool Activo { get => _activo; set => SetProperty(ref _activo, value); }
        public string MensajeError { get => _mensajeError; set { SetProperty(ref _mensajeError, value); TieneError = !string.IsNullOrEmpty(value); } }
        public bool TieneError { get => _tieneError; set => SetProperty(ref _tieneError, value); }
        public decimal TotalCostosFijos { get => _totalCostosFijos; set => SetProperty(ref _totalCostosFijos, value); }
        public bool MostrarFormulario { get => _mostrarFormulario; set => SetProperty(ref _mostrarFormulario, value); }
        public bool EsEdicion => _gastoEnEdicionId > 0;
        public string TituloFormulario => EsEdicion ? "Editar gasto fijo" : "Alta de gasto fijo";

        public ICommand GuardarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand CerrarFormularioCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public async Task CargarDatosAsync()
        {
            var gastos = await _gastoFijoService.ObtenerTodosAsync();
            Gastos.Clear();
            foreach (var g in gastos)
                Gastos.Add(g);
            await ActualizarTotalAsync();
        }


    }
}
