using FinanzasFaciles.Models;

namespace FinanzasFaciles.Services;

public class ResumenMensualDto
{
    
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string NombreMes => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy",
        new System.Globalization.CultureInfo("es-AR"));

    
    public decimal TotalCostosFijos { get; set; }
    public decimal UtilidadBruta { get; set; }
    public decimal TotalRetiros { get; set; }
    public decimal UtilidadReal => UtilidadBruta - TotalRetiros;
    public decimal EfectivoDisponible { get; set; }
    public decimal CostosDirectosAcumulados { get; set; }
    public decimal ExcedenteOFaltante => UtilidadReal - TotalCostosFijos;
    public bool SuperoEquilibrio => ExcedenteOFaltante >= 0;

    
    public List<Ingreso> Ingresos { get; set; } = new();
    public List<Retiro> Retiros { get; set; } = new();
    public List<GastoFijo> GastosFijos { get; set; } = new();
}
