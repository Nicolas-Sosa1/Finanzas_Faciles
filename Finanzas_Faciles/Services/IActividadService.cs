using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finanzas_Faciles.Models;

namespace Finanzas_Faciles.Services
{
    /// RF2 - Interfaz del servicio de Actividades (catálogo de productos/servicios).
    /// Responsabilidades:
    /// - CRUD completo del catálogo de actividades.
    /// - El cálculo de precio (RF2.3) se delega a la implementación según ModoPrecio 
    /// - Notificación al Dashboard mediante el evento DatoActualizado cuando hay cambios.

    public interface IActividadService
    {
        Task<IEnumerable<Actividad>> ObtenerTodasAsync();
        /// Obtiene todas las actividades del catálogo, sin filtrar por estado.
        /// Usado para poblar listas, combos y el selector de actividades al registrar ingresos.

        Task<Actividad?> ObtenerPorIdAsync(int id);
        /// Obtiene una actividad por su identificador único.
        /// Útil para edición, validación de existencia antes de registrar ingresos, o carga de detalle.

        Task AgregarAsync(Actividad actividad);
        /// Agrega una nueva actividad al catálogo.
        /// RF2.3: La implementación debe calcular el precio de venta según ModoPrecio 

        Task ActualizarAsync(Actividad actividad);
        /// Actualiza una actividad existente.
        /// RF2.3: Si se modifica CostoDirecto o MargenGanancia, recalcular PrecioVentaSugerido según ModoPrecio.
        
        Task EliminarAsync(int id);
        /// Elimina una actividad por identificador.
        /// La implementación debe validar que no existan ingresos asociados para asegurar la integridad referencial.

        event EventHandler? DatoActualizado;
        /// Evento que se dispara cuando se agrega, actualiza o elimina una actividad.
        /// Permite que el TableroViewModel (Dashboard) recalcule métricas sin acoplamiento directo.
    }
}
