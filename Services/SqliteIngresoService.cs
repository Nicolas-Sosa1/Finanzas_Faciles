using Microsoft.EntityFrameworkCore;
using FinanzasFaciles.Helpers;
using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

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

    public async Task<Ingreso> ActualizarAsync(Ingreso datos)
    {
        if (datos.Id <= 0)
            throw new ValidationException("Ingreso inválido.");
        if (datos.Cantidad <= 0)
            throw new ValidationException("La cantidad debe ser mayor a cero.");

        var actividad = await _actividadService.ObtenerPorIdAsync(datos.ActividadId);
        if (actividad == null)
            throw new ValidationException("La actividad seleccionada no existe.");

        var (montoTotal, fondoOp, utilidad) = SegmentacionIngresoService.CalcularSegmentacion(actividad, datos.Cantidad);

        await using var db = await _factory.CreateDbContextAsync();
        var entidad = await db.Ingresos.FindAsync(datos.Id);
        if (entidad == null)
            throw new ValidationException("No se encontró el ingreso a modificar.");

        entidad.ActividadId = datos.ActividadId;
        entidad.NombreActividad = actividad.Nombre;
        entidad.Cantidad = datos.Cantidad;
        entidad.Fecha = datos.Fecha;
        entidad.MontoTotal = montoTotal;
        entidad.FondoOperacion = fondoOp;
        entidad.UtilidadBruta = utilidad;

        await db.SaveChangesAsync();
        IngresoRegistrado?.Invoke(this, EventArgs.Empty);
        return entidad;
    }

    public async Task EliminarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entidad = await db.Ingresos.FindAsync(id);
        if (entidad == null)
            return;
        db.Ingresos.Remove(entidad);
        await db.SaveChangesAsync();
        IngresoRegistrado?.Invoke(this, EventArgs.Empty);
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
