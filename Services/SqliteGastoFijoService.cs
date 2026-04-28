using Microsoft.EntityFrameworkCore;
using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public class SqliteGastoFijoService : IGastoFijoService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public event EventHandler? DatoActualizado;

    public SqliteGastoFijoService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<GastoFijo>> ObtenerTodosAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GastosFijos.OrderBy(g => g.Nombre).ToListAsync();
    }

    public async Task<GastoFijo?> ObtenerPorIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GastosFijos.FindAsync(id);
    }

    public async Task AgregarAsync(GastoFijo gasto)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.GastosFijos.Add(gasto);
        await db.SaveChangesAsync();
        DatoActualizado?.Invoke(this, EventArgs.Empty);
    }

    public async Task ActualizarAsync(GastoFijo gasto)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.GastosFijos.Update(gasto);
        await db.SaveChangesAsync();
        DatoActualizado?.Invoke(this, EventArgs.Empty);
    }

    public async Task EliminarAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var gasto = await db.GastosFijos.FindAsync(id);
        if (gasto != null)
        {
            db.GastosFijos.Remove(gasto);
            await db.SaveChangesAsync();
            DatoActualizado?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task<decimal> ObtenerTotalCostosFijosMensualAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.GastosFijos.Where(g => g.Activo).SumAsync(g => g.MontoMensual);
    }
}
