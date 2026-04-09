using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanzas_Faciles.Models;
using Finanzas_Faciles.Services;

namespace Finanzas_Faciles.ViewModels
{
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

        /// RF1.4 - Total de costos fijos mensuales (umbral de rentabilidad).
        public decimal TotalCostosFijos
        {
            get => _totalCostosFijos;
            set => SetProperty(ref _totalCostosFijos, value);
        }

        /// RF4.2 - Utilidad bruta acumulada del período.
        public decimal UtilidadBrutaAcumulada
        {
            get => _utilidadBrutaAcumulada;
            set => SetProperty(ref _utilidadBrutaAcumulada, value);
        }

        /// RF7.2 - Total de retiros personales del período.
        public decimal TotalRetiros
        {
            get => _totalRetiros;
            set => SetProperty(ref _totalRetiros, value);
        }

        /// RF7.3 - Utilidad real = Utilidad bruta - Retiros.
        public decimal UtilidadReal
        {
            get => _utilidadReal;
            set => SetProperty(ref _utilidadReal, value);
        }

        /// RF7.4 - Progreso hacia punto de equilibrio (0-1). Para ProgressBar.
        public double ProgresoCobertura
        {
            get => _progresoCobertura;
            set => SetProperty(ref _progresoCobertura, value);
        }
    }
}
