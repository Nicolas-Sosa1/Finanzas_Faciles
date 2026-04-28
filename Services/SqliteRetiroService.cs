using Microsoft.EntityFrameworkCore;
using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

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

    public async Task<ResultadoRetiro> ActualizarRetiroAsync(int id, decimal monto, DateTime fecha, string concepto, bool confirmarAdvertenciaCapital = false)
    {
        if (monto <= 0)
            return new ResultadoRetiro(false, null, "El monto debe ser mayor a cero.", false);
        if (string.IsNullOrWhiteSpace(concepto))
            return new ResultadoRetiro(false, null, "Debe ingresar un concepto o descripción.", false);

        var c = concepto.Trim();

        await using var db = await _factory.CreateDbContextAsync();
        var r = await db.Retiros.FindAsync(id);
        if (r is null)
            return new ResultadoRetiro(false, null, "No se encontró el retiro.", false);

        var estado = await ObtenerEstadoFinancieroAsync();
        
        var disponibleConDevolucion = estado.EfectivoDisponible + r.Monto;
        if (monto > disponibleConDevolucion)
            return new ResultadoRetiro(false, null,
                $"El monto solicitado ({monto:C2}) supera el efectivo disponible ({disponibleConDevolucion:C2}) considerando el retiro actual.",
                false);

        var efectivoTrasCambio = disponibleConDevolucion - monto;
        if (efectivoTrasCambio < estado.CostosDirectosAcumulados && estado.CostosDirectosAcumulados > 0)
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
        var montoAnterior = r.Monto;
        r.Monto = monto;
        r.Fecha = fecha;
        r.Concepto = c;
        r.TipoRetiro = tipoRetiro;
        r.EstadoPuntoEquilibrioAlMomento = $"{estadoPE} ({estado.Excedente:C2})";
        r.EfectivoDisponibleAlMomento = estado.EfectivoDisponible + montoAnterior;
        r.UtilidadBrutaAlMomento = estado.UtilidadBrutaAcumulada;
        r.CostosFijosAlMomento = estado.CostosFijos;
        r.ExcedenteAlMomento = estado.Excedente;
        r.FechaHoraRegistro = DateTime.UtcNow;

        await db.SaveChangesAsync();
        RetiroRegistrado?.Invoke(this, EventArgs.Empty);
        return new ResultadoRetiro(true, r, null, false);
    }

    public async Task EliminarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var r = await db.Retiros.FindAsync(id);
        if (r is null) return;
        db.Retiros.Remove(r);
        await db.SaveChangesAsync();
        RetiroRegistrado?.Invoke(this, EventArgs.Empty);
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
