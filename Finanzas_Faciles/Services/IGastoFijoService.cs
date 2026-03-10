using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services
{
    /// RF1 - Interfaz del servicio de Gastos Fijos
    /// Evento DatoActualizado para que el Dashboard se recalcule automáticamente
    /// Servicio de Gastos Fijos (SQLite)
    public interface IGastoFijoService
    {
        /// Obtiene todos los gastos fijos registrados
        Task<IEnumerable<GastoFijo>> ObtenerTodosAsync();

        /// Obtiene un gasto fijo por su identificador
        Task<GastoFijo?> ObtenerPorIdAsync(int id);

        /// Agrega un nuevo gasto fijo
        Task AgregarAsync(GastoFijo gasto);

        /// Actualiza un gasto fijo existente
        Task ActualizarAsync(GastoFijo gasto);

        /// Elimina un gasto fijo por su ID
        Task EliminarAsync(int id);

        /// Se dispara al agregar, actualizar o eliminar. El Dashboard se recalcula
        event EventHandler? DatoActualizado;

        /// RF1.3 - Suma todos los gastos activos para obtener el total mensual.
        /// Utilizado como umbral de rentabilidad en el tablero de contro
        Task<decimal> ObtenerTotalCostosFijosMensualAsync();
    }

}
