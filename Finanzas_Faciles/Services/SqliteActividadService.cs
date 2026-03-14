using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services
{
    public class SqliteActividadService : IActividadService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public event EventHandler? DatoActualizado;

        public SqliteActividadService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Actividad>> ObtenerTodasAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Actividades.OrderBy(a => a.Nombre).ToListAsync();
        }

        public async Task<Actividad?> ObtenerPorIdAsync(int id)
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Actividades.FindAsync(id);
        }

        public async Task AgregarAsync(Actividad actividad)
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.Actividades.Add(actividad);
            await db.SaveChangesAsync();
            DatoActualizado?.Invoke(this, EventArgs.Empty);
        }

        public async Task ActualizarAsync(Actividad actividad)
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.Actividades.Update(actividad);
            await db.SaveChangesAsync();
            DatoActualizado?.Invoke(this, EventArgs.Empty);
        }

        public async Task EliminarAsync(int id)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var actividad = await db.Actividades.FindAsync(id);
            if (actividad != null)
            {
                db.Actividades.Remove(actividad);
                await db.SaveChangesAsync();
                DatoActualizado?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
