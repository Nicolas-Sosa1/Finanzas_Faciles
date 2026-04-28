namespace FinanzasFaciles.Services;

public interface IExportService
{
        Task<string> ExportarResumenMensualPdfAsync(ResumenMensualDto resumen);

        Task<string> ExportarResumenMensualExcelAsync(ResumenMensualDto resumen);
}
