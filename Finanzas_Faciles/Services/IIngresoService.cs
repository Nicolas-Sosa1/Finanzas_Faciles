using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services;


/// RF3 - Interfaz del servicio de Registro de Ingresos.
/// Conecta con Actividades (RF2) y alimenta el Tablero (RF4).

public interface IIngresoService
{
    /// Registra un nuevo ingreso con segmentación automática.
    /// RF3.5: Valida existencia de actividad y valores positivos.
    Task<Ingreso> RegistrarAsync(Ingreso ingreso);


    /// Obtiene el historial de ingresos del período actual (mes en curso).

    Task<IEnumerable<Ingreso>> ObtenerIngresosDelPeriodoAsync();


    /// RF3.3 - Utilidad bruta acumulada del período. Para cobertura de costos fijos (RF4).

    Task<decimal> ObtenerUtilidadBrutaAcumuladaMensualAsync();


    /// RF4.2 - Costos directos acumulados del período (capital de trabajo).

    Task<decimal> ObtenerCostosDirectosAcumuladosMensualAsync();


    /// RF3.3 - Saldo total de caja (suma de MontoTotal de todos los ingresos).

    Task<decimal> ObtenerSaldoCajaAsync();

    /// Evento que se dispara al registrar un ingreso. Permite actualizar el Tablero automáticamente.


    event EventHandler? IngresoRegistrado;
}
