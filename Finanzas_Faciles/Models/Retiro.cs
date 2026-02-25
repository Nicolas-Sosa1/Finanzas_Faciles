using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanzas_Faciles.Models
{
    public class Retiro
    {
        public int Id { get; set; }
        public DateTime FechaHoraRegistro { get; set; } = DateTime.UtcNow;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public TipoRetiro TipoRetiro { get; set; }
        public string EstadoPuntoEquilibrioAlMomento { get; set; } = string.Empty;
        public decimal EfectivoDisponibleAlMomento { get; set; }
        public decimal UtilidadBrutaAlMomento { get; set; }
        public decimal CostosFijosAlMomento { get; set; }
        public decimal ExcedenteAlMomento { get; set; }
    }
}
