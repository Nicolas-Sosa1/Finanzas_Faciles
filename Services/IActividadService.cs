using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public interface IActividadService
{
        Task<IEnumerable<Actividad>> ObtenerTodasAsync();

        Task<Actividad?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Actividad actividad);

        Task ActualizarAsync(Actividad actividad);

        event EventHandler? DatoActualizado;

        Task EliminarAsync(int id);
}
