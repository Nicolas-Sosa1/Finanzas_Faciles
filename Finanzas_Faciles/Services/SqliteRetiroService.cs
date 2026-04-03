using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services
{
    public class SqliteRetiroService : IRetiroService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IIngresoService _ingresoService;
        private readonly IGastoFijoService _gastoFijoService;

        public event EventHandler? RetiroRegistrado;

        public SqliteRetiroService(IDbContextFactory<AppDbContext> factory,
            IIngresoService ingresoService, IGastoFijoService gastoFijoService)
        {
            _factory = factory;
            _ingresoService = ingresoService;
            _gastoFijoService = gastoFijoService;
        }

        public async Task<EstadoFinancieroActual> ObtenerEstadoFinancieroAsync()
        {
            var saldoCaja = await _ingresoService.ObtenerSaldoCajaAsync();
            var totalRetirado = await ObtenerTotalRetiradoAsync();
            var utilidadBruta = await _ingresoService.ObtenerUtilidadBrutaAcumuladaMensualAsync();
            var costosFijos = await _gastoFijoService.ObtenerTotalCostosFijosMensualAsync();
            var costosDirectos = await _ingresoService.ObtenerCostosDirectosAcumuladosMensualAsync();
            var excedente = utilidadBruta - costosFijos;
            var efectivoDisponible = saldoCaja - totalRetirado;

            return new EstadoFinancieroActual(
                saldoCaja, totalRetirado, efectivoDisponible,
                utilidadBruta, costosFijos, costosDirectos, excedente);
        }

        public async Task<ResultadoRetiro> RegistrarRetiroAsync(decimal monto, DateTime fecha, string concepto, bool confirmarAdvertenciaCapital = false)
        {
            if (monto <= 0)
                return new ResultadoRetiro(false, null, "El monto debe ser mayor a cero.", false);

            var estado = await ObtenerEstadoFinancieroAsync();

            if (monto > estado.EfectivoDisponible)
                return new ResultadoRetiro(false, null,
                    $"El monto solicitado ({monto:C2}) supera el efectivo disponible ({estado.EfectivoDisponible:C2}).", false);

            var efectivoDespues = estado.EfectivoDisponible - monto;
            if (efectivoDespues < estado.CostosDirectosAcumulados && estado.CostosDirectosAcumulados > 0)
            {
                if (!confirmarAdvertenciaCapital)
                    return new ResultadoRetiro(false, null,
                        "Este retiro comprometería el capital de operación necesario para la continuidad del negocio. ¿Desea continuar igual?",
                        true);
            }

            var tipoRetiro = estado.Excedente > 0 && monto <= estado.Excedente
                ? TipoRetiro.GananciaReal
                : TipoRetiro.AdelantoUtilidad;

            var estadoPE = estado.Excedente > 0 ? "Superávit" : estado.Excedente < 0 ? "Déficit" : "Equilibrio";

            var retiro = new Retiro
            {
                FechaHoraRegistro = DateTime.UtcNow,
                Monto = monto,
                Fecha = fecha,
                Concepto = concepto.Trim(),
                TipoRetiro = tipoRetiro,
                EstadoPuntoEquilibrioAlMomento = $"{estadoPE} ({estado.Excedente:C2})",
                EfectivoDisponibleAlMomento = estado.EfectivoDisponible,
                UtilidadBrutaAlMomento = estado.UtilidadBrutaAcumulada,
                CostosFijosAlMomento = estado.CostosFijos,
                ExcedenteAlMomento = estado.Excedente
            };

            await using var db = await _factory.CreateDbContextAsync();
            db.Retiros.Add(retiro);
            await db.SaveChangesAsync();
            RetiroRegistrado?.Invoke(this, EventArgs.Empty);
            return new ResultadoRetiro(true, retiro, null, false);
        }

        public async Task<IEnumerable<Retiro>> ObtenerTodosAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Retiros.OrderByDescending(r => r.FechaHoraRegistro).ToListAsync();
        }

        public async Task<IEnumerable<Retiro>> ObtenerPorPeriodoAsync(DateTime desde, DateTime hasta)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var hastaFin = hasta.Date.AddDays(1);
            return await db.Retiros
                .Where(r => r.Fecha >= desde.Date && r.Fecha < hastaFin)
                .OrderByDescending(r => r.FechaHoraRegistro)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerTotalRetiradoEnPeriodoAsync(DateTime desde, DateTime hasta)
        {
            var retiros = await ObtenerPorPeriodoAsync(desde, hasta);
            return retiros.Sum(r => r.Monto);
        }

        public async Task<decimal> ObtenerTotalRetiradoAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Retiros.SumAsync(r => r.Monto);
        }
    }
}
