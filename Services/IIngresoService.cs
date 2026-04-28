using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public interface IIngresoService
{
        Task<Ingreso> RegistrarAsync(Ingreso ingreso);

        Task<Ingreso> ActualizarAsync(Ingreso ingreso);

        Task EliminarAsync(int id);

        Task<IEnumerable<Ingreso>> ObtenerIngresosDelPeriodoAsync();

        Task<decimal> ObtenerUtilidadBrutaAcumuladaMensualAsync();

        Task<decimal> ObtenerCostosDirectosAcumuladosMensualAsync();

        Task<decimal> ObtenerSaldoCajaAsync();

        event EventHandler? IngresoRegistrado;
}
