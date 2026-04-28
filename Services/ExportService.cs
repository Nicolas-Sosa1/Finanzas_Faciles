using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QColors = QuestPDF.Helpers.Colors;
using QContainer = QuestPDF.Infrastructure.IContainer;

namespace FinanzasFaciles.Services;

public class ExportService : IExportService
{
    
    private static readonly string ColorPrimario  = "#1A3C5E";   
    private static readonly string ColorExito     = "#27AE60";
    private static readonly string ColorPeligro   = "#E74C3C";
    private static readonly string ColorMuted     = "#7F8C8D";
    private static readonly string ColorFondo     = "#F4F6F8";

    
    
    

    public Task<string> ExportarResumenMensualPdfAsync(ResumenMensualDto resumen)
    {
        
        
        QuestPDF.Settings.License = LicenseType.Community;

        var fileName = $"ResumenMensual_{resumen.Anio}_{resumen.Mes:D2}.pdf";
        var filePath = ObtenerRutaExportacion(fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(ts => ts.FontSize(10).FontFamily("Arial"));

                
                page.Header().Element(ComposeHeader(resumen));

                
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    
                    col.Item().Element(ComposeIndicadores(resumen));

                    
                    if (resumen.GastosFijos.Any())
                    {
                        col.Item().Element(ComposeSeccion("Costos Fijos Mensuales",
                            ComposeTablaGastosFijos(resumen)));
                    }

                    
                    if (resumen.Ingresos.Any())
                    {
                        col.Item().Element(ComposeSeccion("Detalle de Ingresos",
                            ComposeTablaIngresos(resumen)));
                    }

                    
                    if (resumen.Retiros.Any())
                    {
                        col.Item().Element(ComposeSeccion("Detalle de Retiros",
                            ComposeTablaRetiros(resumen)));
                    }
                });

                
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Generado por Finanzas Fáciles · ").FontColor(ColorMuted);
                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontColor(ColorMuted);
                    txt.Span("   Página ").FontColor(ColorMuted);
                    txt.CurrentPageNumber().FontColor(ColorMuted);
                    txt.Span(" de ").FontColor(ColorMuted);
                    txt.TotalPages().FontColor(ColorMuted);
                });
            });
        });

        document.GeneratePdf(filePath);
        return Task.FromResult(filePath);
    }

    

    private static Action<QContainer> ComposeHeader(ResumenMensualDto r) =>
    c => c.Row(row =>
    {
        row.RelativeItem().Column(col =>
        {
            col.Item().Text("Finanzas Fáciles")
                .FontSize(20).Bold().FontColor(ColorPrimario);
            col.Item().Text($"Resumen Mensual — {r.NombreMes}")
                .FontSize(13).FontColor(ColorMuted);
        });
        row.ConstantItem(120).AlignRight().AlignMiddle()
            .Text(r.SuperoEquilibrio ? "EQUILIBRIO ALCANZADO" : "DÉFICIT")
            .FontSize(9).Bold()
            .FontColor(r.SuperoEquilibrio ? ColorExito : ColorPeligro);
    });

    private static Action<QContainer> ComposeIndicadores(ResumenMensualDto r) =>
    c => c.Background(ColorFondo).Padding(12).Column(col =>
    {
        col.Item().Text("Indicadores del Período").FontSize(11).Bold().FontColor(ColorPrimario);
        col.Item().Height(6);
        col.Item().Row(row =>
        {
            AgregarIndicador(row, "Costos Fijos", r.TotalCostosFijos, null);
            AgregarIndicador(row, "Utilidad Bruta", r.UtilidadBruta, null);
            AgregarIndicador(row, "Total Retiros", r.TotalRetiros, null);
            AgregarIndicador(row, "Utilidad Real", r.UtilidadReal,
                r.UtilidadReal >= 0 ? ColorExito : ColorPeligro);
        });
        col.Item().Height(6);
        col.Item().Row(row =>
        {
            AgregarIndicador(row, "Capital de Operación", r.CostosDirectosAcumulados, null);
            AgregarIndicador(row, "Efectivo Disponible", r.EfectivoDisponible, null);
            AgregarIndicador(row, r.SuperoEquilibrio ? "Superávit" : "Déficit",
                Math.Abs(r.ExcedenteOFaltante),
                r.SuperoEquilibrio ? ColorExito : ColorPeligro);
            row.RelativeItem();
        });
    });

    private static void AgregarIndicador(RowDescriptor row, string titulo, decimal valor, string? color)
    {
        row.RelativeItem().Column(col =>
        {
            col.Item().Text(titulo).FontSize(8).FontColor(ColorMuted);
            col.Item().Text(valor.ToString("C2")).FontSize(12).Bold()
                .FontColor(color ?? "#000000");
        });
    }

    private static Action<QContainer> ComposeSeccion(string titulo,
    Action<QContainer> contenido) =>
    c => c.Column(col =>
    {
        col.Item().BorderBottom(1).BorderColor(ColorPrimario).PaddingBottom(3)
            .Text(titulo).FontSize(11).Bold().FontColor(ColorPrimario);
        col.Item().Height(6);
        col.Item().Element(contenido);
    });

    private static Action<QContainer> ComposeTablaGastosFijos(ResumenMensualDto r) =>
    c => c.Table(table =>
    {
        table.ColumnsDefinition(cols =>
        {
            cols.RelativeColumn(3);
            cols.RelativeColumn(2);
            cols.RelativeColumn(1);
        });

        TablaEncabezado(table, "Nombre", "Categoría", "Monto/mes");

        bool par = false;
        foreach (var g in r.GastosFijos.Where(x => x.Activo))
        {
            var bg = par ? ColorFondo : "#FFFFFF";
            par = !par;
            table.Cell().Background(bg).Padding(4).Text(g.Nombre);
            table.Cell().Background(bg).Padding(4).Text(g.Categoria.ToString());
            table.Cell().Background(bg).Padding(4).AlignRight()
                .Text(g.MontoMensual.ToString("C2"));
        }

        table.Cell().ColumnSpan(2).Padding(4).AlignRight()
            .Text("TOTAL").Bold();
        table.Cell().Padding(4).AlignRight()
            .Text(r.TotalCostosFijos.ToString("C2")).Bold().FontColor(ColorPrimario);
    });

    private static Action<QContainer> ComposeTablaIngresos(ResumenMensualDto r) =>
    c => c.Table(table =>
    {
        table.ColumnsDefinition(cols =>
        {
            cols.RelativeColumn(2);
            cols.ConstantColumn(55);
            cols.ConstantColumn(35);
            cols.RelativeColumn(1);
            cols.RelativeColumn(1);
            cols.RelativeColumn(1);
        });

        TablaEncabezado(table, "Actividad", "Fecha", "Cant.", "Total", "Fdo. Op.", "Utilidad");

        bool par = false;
        foreach (var i in r.Ingresos.OrderBy(x => x.Fecha))
        {
            var bg = par ? ColorFondo : "#FFFFFF";
            par = !par;
            table.Cell().Background(bg).Padding(3).Text(i.NombreActividad);
            table.Cell().Background(bg).Padding(3).AlignCenter()
                .Text(i.Fecha.ToString("dd/MM/yy"));
            table.Cell().Background(bg).Padding(3).AlignCenter()
                .Text(i.Cantidad.ToString());
            table.Cell().Background(bg).Padding(3).AlignRight()
                .Text(i.MontoTotal.ToString("C2"));
            table.Cell().Background(bg).Padding(3).AlignRight()
                .Text(i.FondoOperacion.ToString("C2"));
            table.Cell().Background(bg).Padding(3).AlignRight()
                .Text(i.UtilidadBruta.ToString("C2")).Bold();
        }

        table.Cell().ColumnSpan(3).Padding(3).AlignRight().Text("TOTAL").Bold();
        table.Cell().Padding(3).AlignRight()
            .Text(r.Ingresos.Sum(i => i.MontoTotal).ToString("C2")).Bold();
        table.Cell().Padding(3).AlignRight()
            .Text(r.Ingresos.Sum(i => i.FondoOperacion).ToString("C2")).Bold();
        table.Cell().Padding(3).AlignRight()
            .Text(r.UtilidadBruta.ToString("C2")).Bold().FontColor(ColorExito);
    });

    private static Action<QContainer> ComposeTablaRetiros(ResumenMensualDto r) =>
    c => c.Table(table =>
    {
        table.ColumnsDefinition(cols =>
        {
            cols.RelativeColumn(3);
            cols.ConstantColumn(65);
            cols.RelativeColumn(1);
            cols.RelativeColumn(2);
        });

        TablaEncabezado(table, "Concepto", "Fecha", "Monto", "Tipo");

        bool par = false;
        foreach (var retiro in r.Retiros.OrderBy(x => x.Fecha))
        {
            var bg = par ? ColorFondo : "#FFFFFF";
            par = !par;
            table.Cell().Background(bg).Padding(3).Text(retiro.Concepto);
            table.Cell().Background(bg).Padding(3).AlignCenter()
                .Text(retiro.Fecha.ToString("dd/MM/yy"));
            table.Cell().Background(bg).Padding(3).AlignRight()
                .Text(retiro.Monto.ToString("C2"));
            table.Cell().Background(bg).Padding(3)
                .Text(retiro.TipoRetiro.ToString()).FontColor(ColorMuted);
        }

        table.Cell().ColumnSpan(2).Padding(3).AlignRight().Text("TOTAL").Bold();
        table.Cell().Padding(3).AlignRight()
            .Text(r.TotalRetiros.ToString("C2")).Bold().FontColor(ColorPeligro);
        table.Cell();
    });

    private static void TablaEncabezado(TableDescriptor table, params string[] columnas)
    {
        
        table.Header(h =>
        {
            
            foreach (var col in columnas)
            {
                h.Cell().Background(ColorPrimario).Padding(5)
                 .Text(col).FontColor(QColors.White).Bold().FontSize(9);
            }
        });
    }

    
    
    

    public Task<string> ExportarResumenMensualExcelAsync(ResumenMensualDto resumen)
    {
        var fileName = $"ResumenMensual_{resumen.Anio}_{resumen.Mes:D2}.xlsx";
        var filePath = ObtenerRutaExportacion(fileName);

        using var wb = new XLWorkbook();

        AgregarHojaResumen(wb, resumen);
        AgregarHojaIngresos(wb, resumen);
        AgregarHojaRetiros(wb, resumen);
        AgregarHojaGastosFijos(wb, resumen);

        wb.SaveAs(filePath);
        return Task.FromResult(filePath);
    }

    private static void AgregarHojaResumen(XLWorkbook wb, ResumenMensualDto r)
    {
        var ws = wb.Worksheets.Add("Resumen");
        ws.Column(1).Width = 30;
        ws.Column(2).Width = 18;

        
        ws.Cell(1, 1).Value = $"Finanzas Fáciles — {r.NombreMes}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(ColorPrimario);
        ws.Range(1, 1, 1, 2).Merge();

        var filas = new (string Label, decimal Valor, bool Bold, string? Color)[]
        {
            ("Costos Fijos Mensuales", r.TotalCostosFijos, false, null),
            ("Utilidad Bruta Acumulada", r.UtilidadBruta, false, null),
            ("Total Retiros", r.TotalRetiros, false, null),
            ("Utilidad Real (Bruta − Retiros)", r.UtilidadReal, true,
                r.UtilidadReal >= 0 ? ColorExito : ColorPeligro),
            ("Capital de Operación", r.CostosDirectosAcumulados, false, null),
            ("Efectivo Disponible", r.EfectivoDisponible, false, null),
            (r.SuperoEquilibrio ? "Superávit" : "Déficit (falta para equilibrio)",
                Math.Abs(r.ExcedenteOFaltante), true,
                r.SuperoEquilibrio ? ColorExito : ColorPeligro),
        };

        for (int i = 0; i < filas.Length; i++)
        {
            var fila = i + 3;
            var (label, valor, bold, color) = filas[i];
            ws.Cell(fila, 1).Value = label;
            ws.Cell(fila, 2).Value = valor;
            ws.Cell(fila, 2).Style.NumberFormat.Format = "$ #,##0.00";
            if (bold)
            {
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
            }
            if (color != null)
                ws.Cell(fila, 2).Style.Font.FontColor = XLColor.FromHtml(color);
        }

        
        var estadoFila = filas.Length + 4;
        ws.Cell(estadoFila, 1).Value = "Estado del período:";
        ws.Cell(estadoFila, 2).Value = r.SuperoEquilibrio
            ? "✔ Equilibrio alcanzado" : "⚠ Déficit — no se cubrieron los costos fijos";
        ws.Cell(estadoFila, 2).Style.Font.FontColor =
            XLColor.FromHtml(r.SuperoEquilibrio ? ColorExito : ColorPeligro);
        ws.Cell(estadoFila, 2).Style.Font.Bold = true;

        EstilizarHoja(ws, 1, filas.Length + 2);
    }

    private static void AgregarHojaIngresos(XLWorkbook wb, ResumenMensualDto r)
    {
        var ws = wb.Worksheets.Add("Ingresos");
        string[] headers = { "Actividad", "Fecha", "Cantidad", "Monto Total", "Fondo Operación", "Utilidad Bruta" };
        int[] anchos = { 28, 12, 10, 15, 16, 15 };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Column(i + 1).Width = anchos[i];
        }

        int fila = 2;
        foreach (var ing in r.Ingresos.OrderBy(x => x.Fecha))
        {
            ws.Cell(fila, 1).Value = ing.NombreActividad;
            ws.Cell(fila, 2).Value = ing.Fecha.ToString("dd/MM/yyyy");
            ws.Cell(fila, 3).Value = ing.Cantidad;
            ws.Cell(fila, 4).Value = ing.MontoTotal;
            ws.Cell(fila, 5).Value = ing.FondoOperacion;
            ws.Cell(fila, 6).Value = ing.UtilidadBruta;
            foreach (var col in new[] { 4, 5, 6 })
                ws.Cell(fila, col).Style.NumberFormat.Format = "$ #,##0.00";
            fila++;
        }

        
        ws.Cell(fila, 1).Value = "TOTAL";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila, 4).Value = r.Ingresos.Sum(i => i.MontoTotal);
        ws.Cell(fila, 5).Value = r.Ingresos.Sum(i => i.FondoOperacion);
        ws.Cell(fila, 6).Value = r.UtilidadBruta;
        foreach (var col in new[] { 4, 5, 6 })
        {
            ws.Cell(fila, col).Style.NumberFormat.Format = "$ #,##0.00";
            ws.Cell(fila, col).Style.Font.Bold = true;
        }

        EstilizarHoja(ws, 1, fila);
    }

    private static void AgregarHojaRetiros(XLWorkbook wb, ResumenMensualDto r)
    {
        var ws = wb.Worksheets.Add("Retiros");
        string[] headers = { "Concepto", "Fecha", "Monto", "Tipo", "PE al momento" };
        int[] anchos = { 30, 12, 14, 18, 30 };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Column(i + 1).Width = anchos[i];
        }

        int fila = 2;
        foreach (var ret in r.Retiros.OrderBy(x => x.Fecha))
        {
            ws.Cell(fila, 1).Value = ret.Concepto;
            ws.Cell(fila, 2).Value = ret.Fecha.ToString("dd/MM/yyyy");
            ws.Cell(fila, 3).Value = ret.Monto;
            ws.Cell(fila, 3).Style.NumberFormat.Format = "$ #,##0.00";
            ws.Cell(fila, 4).Value = ret.TipoRetiro.ToString();
            ws.Cell(fila, 5).Value = ret.EstadoPuntoEquilibrioAlMomento;
            fila++;
        }

        
        ws.Cell(fila, 1).Value = "TOTAL";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila, 3).Value = r.TotalRetiros;
        ws.Cell(fila, 3).Style.NumberFormat.Format = "$ #,##0.00";
        ws.Cell(fila, 3).Style.Font.Bold = true;

        EstilizarHoja(ws, 1, fila);
    }

    private static void AgregarHojaGastosFijos(XLWorkbook wb, ResumenMensualDto r)
    {
        var ws = wb.Worksheets.Add("Costos Fijos");
        string[] headers = { "Nombre", "Categoría", "Monto Mensual", "Activo" };
        int[] anchos = { 28, 18, 16, 10 };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Column(i + 1).Width = anchos[i];
        }

        int fila = 2;
        foreach (var g in r.GastosFijos)
        {
            ws.Cell(fila, 1).Value = g.Nombre;
            ws.Cell(fila, 2).Value = g.Categoria.ToString();
            ws.Cell(fila, 3).Value = g.MontoMensual;
            ws.Cell(fila, 3).Style.NumberFormat.Format = "$ #,##0.00";
            ws.Cell(fila, 4).Value = g.Activo ? "Sí" : "No";
            fila++;
        }

        
        ws.Cell(fila, 1).Value = "TOTAL (activos)";
        ws.Cell(fila, 1).Style.Font.Bold = true;
        ws.Cell(fila, 3).Value = r.TotalCostosFijos;
        ws.Cell(fila, 3).Style.NumberFormat.Format = "$ #,##0.00";
        ws.Cell(fila, 3).Style.Font.Bold = true;

        EstilizarHoja(ws, 1, fila);
    }

    

        private static void EstilizarHoja(IXLWorksheet ws, int headerRow, int lastRow)
    {
        var headerRange = ws.Row(headerRow).CellsUsed();
        foreach (var cell in headerRange)
        {
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(ColorPrimario);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        
        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            if (r % 2 == 0)
                ws.Row(r).Style.Fill.BackgroundColor = XLColor.FromHtml(ColorFondo);
        }
    }

        private static string ObtenerRutaExportacion(string fileName)
    {
#if ANDROID || IOS
        return Path.Combine(FileSystem.CacheDirectory, fileName);
#else
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var carpeta = Path.Combine(docs, "FinanzasFaciles");
        Directory.CreateDirectory(carpeta);
        return Path.Combine(carpeta, fileName);
#endif
    }
}
