using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanzas_Faciles.Models
{
    public class Actividad
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public decimal CostoDirecto { get; set; }
        public decimal MargenGanancia { get; set; }
        public EstadoActividad Estado { get; set; } = EstadoActividad.Activa;
        public ModoPrecioActividad ModoPrecio { get; set; } = ModoPrecioActividad.PorMargen;
        public decimal? PrecioVentaFijo { get; set; }

        public decimal PrecioVentaSugerido =>
            ModoPrecio == ModoPrecioActividad.PrecioFijo && PrecioVentaFijo.HasValue
                ? Math.Round(PrecioVentaFijo.Value, 2)
                : CostoDirecto > 0
                    ? Math.Round(CostoDirecto * (1 + MargenGanancia / 100m), 2)
                    : 0;
        public decimal MargenCalculado =>
            ModoPrecio == ModoPrecioActividad.PrecioFijo && PrecioVentaFijo.HasValue && CostoDirecto > 0
                ? Math.Round(((PrecioVentaFijo.Value - CostoDirecto) / CostoDirecto) * 100m, 2)
                : MargenGanancia;
        public decimal UtilidadPorUnidad => PrecioVentaSugerido - CostoDirecto;
    }
}
