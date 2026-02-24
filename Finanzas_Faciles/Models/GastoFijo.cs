using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanzas_Faciles.Models
{
    public class GastoFijo
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal MontoMensual { get; set; }

        public CategoriaGastoFijo Categoria { get; set; }

        public bool Activo { get; set; } = true;
    }
}
