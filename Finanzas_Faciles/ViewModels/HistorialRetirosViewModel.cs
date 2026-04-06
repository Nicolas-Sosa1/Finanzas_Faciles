using Finanzas_Faciles.Helpers;
using Finanzas_Faciles.Models;
using Finanzas_Faciles.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Finanzas_Faciles.ViewModels
{
    public class HistorialRetirosViewModel : BaseViewModel
    {
        private readonly IRetiroService _retiroService;
        private readonly IIngresoService _ingresoService;
        private PeriodoFiltro _periodoSeleccionado = PeriodoFiltro.Mes;
        private decimal _totalRetiradoEnPeriodo;
        private decimal _utilidadRealEnPeriodo;
        private string _mensajeConfirmacion = string.Empty;

        public HistorialRetirosViewModel(IRetiroService retiroService, IIngresoService ingresoService)
        {
            _retiroService = retiroService;
            _ingresoService = ingresoService;
            Retiros = new ObservableCollection<Retiro>();
            PeriodosDisponibles = Enum.GetValues<PeriodoFiltro>();
            FiltrarCommand = new AsyncRelayCommand(FiltrarAsync);
        }

        public ObservableCollection<Retiro> Retiros { get; }
        public PeriodoFiltro[] PeriodosDisponibles { get; }

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

        public async Task CargarDatosAsync()
        {
            await FiltrarAsync();
        }

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
}
