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

        /// RF7.4 - Estado financiero: "Fase de Cobertura" o "Ganancia Neta Disponible".
        public string EstadoFinanciero =>
            SinCostosFijos ? "Sin configurar" :
            TieneExcedente ? "Ganancia Neta Disponible" : "Fase de Cobertura";

        /// RF7.4 - Mensaje: monto faltante o excedente disponible.
        public string MensajeEstado =>
            SinCostosFijos ? MensajeAlerta :
            TieneExcedente ? $"Superávit disponible: {ExcedenteOFaltante:C2}" :
            $"Monto pendiente para equilibrio: {Math.Abs(ExcedenteOFaltante):C2}";

        /// RF4.2 - Costos directos acumulados (capital de trabajo).
        public decimal CostosDirectosAcumulados
        {
            get => _costosDirectosAcumulados;
            set => SetProperty(ref _costosDirectosAcumulados, value);
        }

        /// RF3.3 - Saldo total de caja (ingresos).
        public decimal SaldoCaja
        {
            get => _saldoCaja;
            set => SetProperty(ref _saldoCaja, value);
        }

        /// RF5.3 - Efectivo disponible
        public decimal EfectivoDisponible
        {
            get => _efectivoDisponible;
            set => SetProperty(ref _efectivoDisponible, value);
        }

        /// RF4.4 - Monto: excedente (positivo) o faltante para cubrir costos fijos (negativo).
        public decimal ExcedenteOFaltante
        {
            get => _excedenteOFaltante;
            set => SetProperty(ref _excedenteOFaltante, value);
        }

        /// True si hay excedente, False si falta para el punto de equilibrio.
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

        /// RF4.5 - Indica que no hay costos fijos configurados.
        public bool SinCostosFijos
        {
            get => _sinCostosFijos;
            set => SetProperty(ref _sinCostosFijos, value);
        }

        /// True cuando hay alerta pero ya hay costos configurados (evita duplicar mensaje "sin configurar").
        public bool MostrarAlertaAmarilla => TieneAlerta && !SinCostosFijos;

        public async Task CargarDatosAsync()
        {
            await ActualizarAsync();
        }
    }
}
