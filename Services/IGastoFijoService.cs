using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public interface IGastoFijoService
{
        Task<IEnumerable<GastoFijo>> ObtenerTodosAsync();

        Task<GastoFijo?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(GastoFijo gasto);

        Task ActualizarAsync(GastoFijo gasto);

        Task EliminarAsync(int id);

        event EventHandler? DatoActualizado;

        Task<decimal> ObtenerTotalCostosFijosMensualAsync();
}
