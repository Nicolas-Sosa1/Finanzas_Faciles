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

        private async Task GuardarAsync()
        {
            LimpiarError();
            if (string.IsNullOrWhiteSpace(Nombre)) { MensajeError = "Debe ingresar el nombre del gasto fijo."; return; }
            if (string.IsNullOrWhiteSpace(MontoTexto)) { MensajeError = "Debe ingresar el monto mensual."; return; }
            if (!decimal.TryParse(MontoTexto.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var monto) || monto <= 0)
            { MensajeError = "El monto debe ser un valor numérico mayor a cero."; return; }

            try
            {
                if (EsEdicion)
                {
                    var gasto = await _gastoFijoService.ObtenerPorIdAsync(_gastoEnEdicionId);
                    if (gasto == null) { MensajeError = "Gasto no encontrado."; return; }
                    gasto.Nombre = Nombre.Trim();
                    gasto.MontoMensual = monto;
                    gasto.Categoria = CategoriaSeleccionada;
                    gasto.Activo = Activo;
                    await _gastoFijoService.ActualizarAsync(gasto);
                    var idx = Gastos.ToList().FindIndex(g => g.Id == gasto.Id);
                    if (idx >= 0) Gastos[idx] = new GastoFijo { Id = gasto.Id, Nombre = gasto.Nombre, MontoMensual = gasto.MontoMensual, Categoria = gasto.Categoria, Activo = gasto.Activo };
                }
                else
                {
                    var gasto = new GastoFijo { Nombre = Nombre.Trim(), MontoMensual = monto, Categoria = CategoriaSeleccionada, Activo = true };
                    await _gastoFijoService.AgregarAsync(gasto);
                    Gastos.Add(gasto);
                }
                await ActualizarTotalAsync();
                LimpiarFormulario();
                CerrarFormulario();
            }
            catch (ValidationException ex) { MensajeError = ex.Message; }
        }

        private async Task EliminarAsync(GastoFijo? gasto)
        {
            if (gasto == null) return;
            var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
            if (page == null) return;
            var confirmar = await page.DisplayAlert("Confirmar eliminación",
                $"¿Eliminar el gasto '{gasto.Nombre}'? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmar) return;
            await _gastoFijoService.EliminarAsync(gasto.Id);
            Gastos.Remove(gasto);
            await ActualizarTotalAsync();
        }

        private void Editar(GastoFijo? gasto)
        {
            if (gasto == null) return;
            _gastoEnEdicionId = gasto.Id;
            Nombre = gasto.Nombre;
            MontoTexto = gasto.MontoMensual.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            CategoriaSeleccionada = gasto.Categoria;
            Activo = gasto.Activo;
            LimpiarError();
            MostrarFormulario = true;
            OnPropertyChanged(nameof(EsEdicion));
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void AbrirFormularioNuevo()
        {
            _gastoEnEdicionId = 0;
            LimpiarFormulario();
            LimpiarError();
            MostrarFormulario = true;
            OnPropertyChanged(nameof(EsEdicion));
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            _gastoEnEdicionId = 0;
            LimpiarFormulario();
            LimpiarError();
            OnPropertyChanged(nameof(EsEdicion));
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void LimpiarFormulario()
        {
            Nombre = string.Empty;
            MontoTexto = string.Empty;
            CategoriaSeleccionada = CategoriaGastoFijo.Otros;
            Activo = true;
        }

        private void LimpiarError() => MensajeError = string.Empty;

        private async Task ActualizarTotalAsync()
        {
            TotalCostosFijos = await _gastoFijoService.ObtenerTotalCostosFijosMensualAsync();
        }

    }
}
