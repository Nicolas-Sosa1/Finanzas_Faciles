namespace FinanzasFaciles.Models;

public class Ingreso
{
    public int Id { get; set; }

        public int ActividadId { get; set; }

        public string NombreActividad { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; }

        public decimal MontoTotal { get; set; }

        public decimal FondoOperacion { get; set; }

        public decimal UtilidadBruta { get; set; }
}
