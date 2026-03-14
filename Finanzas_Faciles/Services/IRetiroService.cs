using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services;


public interface IRetiroService
{

    Task<ResultadoRetiro> RegistrarRetiroAsync(decimal monto, DateTime fecha, string concepto, bool confirmarAdvertenciaCapital = false);

    Task<EstadoFinancieroActual> ObtenerEstadoFinancieroAsync();


    Task<IEnumerable<Retiro>> ObtenerTodosAsync();


    Task<IEnumerable<Retiro>> ObtenerPorPeriodoAsync(DateTime desde, DateTime hasta);


    Task<decimal> ObtenerTotalRetiradoEnPeriodoAsync(DateTime desde, DateTime hasta);


    Task<decimal> ObtenerTotalRetiradoAsync();


    event EventHandler? RetiroRegistrado;
}
public record EstadoFinancieroActual(
    decimal SaldoCaja,
    decimal TotalRetirado,
    decimal EfectivoDisponible,
    decimal UtilidadBrutaAcumulada,
    decimal CostosFijos,
    decimal CostosDirectosAcumulados,
    decimal Excedente
);

public record ResultadoRetiro(
    bool Exito,
    Retiro? Retiro,
    string? MensajeError,
    bool RequiereConfirmacionCapital
);
