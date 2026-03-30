using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanzas_Faciles.Helpers;
using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services
{
    public class SqliteIngresoService : IIngresoService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IActividadService _actividadService;

        public event EventHandler? IngresoRegistrado;

        public SqliteIngresoService(IDbContextFactory<AppDbContext> factory, IActividadService actividadService)
        {
            _factory = factory;
            _actividadService = actividadService;
        }

        public async Task<Ingreso> RegistrarAsync(Ingreso ingreso)
        {
            var actividad = await _actividadService.ObtenerPorIdAsync(ingreso.ActividadId);
            if (actividad == null)
                throw new ValidationException("La actividad seleccionada no existe.");
            if (ingreso.Cantidad <= 0)
                throw new ValidationException("La cantidad debe ser mayor a cero.");

            ingreso.NombreActividad = actividad.Nombre;
            var (montoTotal, fondoOp, utilidad) = SegmentacionIngresoService.CalcularSegmentacion(actividad, ingreso.Cantidad);
            ingreso.MontoTotal = montoTotal;
            ingreso.FondoOperacion = fondoOp;
            ingreso.UtilidadBruta = utilidad;

            await using var db = await _factory.CreateDbContextAsync();
            db.Ingresos.Add(ingreso);
            await db.SaveChangesAsync();
            IngresoRegistrado?.Invoke(this, EventArgs.Empty);
            return ingreso;
        }

        public async Task<IEnumerable<Ingreso>> ObtenerIngresosDelPeriodoAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);
            return await db.Ingresos
                .Where(i => i.Fecha >= inicioMes && i.Fecha < finMes)
                .OrderByDescending(i => i.Fecha)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerUtilidadBrutaAcumuladaMensualAsync()
        {
            var ingresos = await ObtenerIngresosDelPeriodoAsync();
            return ingresos.Sum(i => i.UtilidadBruta);
        }

        public async Task<decimal> ObtenerCostosDirectosAcumuladosMensualAsync()
        {
            var ingresos = await ObtenerIngresosDelPeriodoAsync();
            return ingresos.Sum(i => i.FondoOperacion);
        }

        public async Task<decimal> ObtenerSaldoCajaAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Ingresos.SumAsync(i => i.MontoTotal);
        }
    }
}
