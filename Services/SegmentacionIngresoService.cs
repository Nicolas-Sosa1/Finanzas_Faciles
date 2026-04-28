using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public static class SegmentacionIngresoService
{
        public static (decimal MontoTotal, decimal FondoOperacion, decimal UtilidadBruta) CalcularSegmentacion(
        Actividad actividad, int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));

        var montoTotal = actividad.PrecioVentaSugerido * cantidad;
        var fondoOperacion = actividad.CostoDirecto * cantidad;
        var utilidadBruta = actividad.UtilidadPorUnidad * cantidad;

        return (Math.Round(montoTotal, 2), Math.Round(fondoOperacion, 2), Math.Round(utilidadBruta, 2));
    }
}
