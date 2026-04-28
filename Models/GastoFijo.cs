namespace FinanzasFaciles.Models;

public class GastoFijo
{
    public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal MontoMensual { get; set; }

        public CategoriaGastoFijo Categoria { get; set; }

        public bool Activo { get; set; } = true;
}
