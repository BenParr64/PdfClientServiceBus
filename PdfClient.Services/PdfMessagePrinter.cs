using PdfClient.Services.Interfaces;
using System.Text.Json;
using PdfPaperSize = IronPdf.Rendering.PdfPaperSize;

namespace PdfClient.Services;

public class PdfMessagePrinter : IPdfMessagePrinter
{
    private readonly string _printerSettingsPath;
    public PdfMessagePrinter(string printerSettingsPath)
    {
        _printerSettingsPath = printerSettingsPath;
    }
    public void CreateAndPrintPdf(string html, PageSizeEnum pageSize)
    {
        var renderer = new ChromePdfRenderer
        {
            RenderingOptions =
            {
                PaperSize = PdfPaperSize.Custom,
                MarginTop = 0,
                MarginRight = 0,
                MarginBottom = 0,
                MarginLeft = 0
            }
        };

        var printerName = GetPrinterName(pageSize);

        switch (pageSize)
        {
            case PageSizeEnum.SixByFour:
            {
                renderer.RenderingOptions.SetCustomPaperSizeinMilimeters(152.4, 101.6);
                break;
            }
            case PageSizeEnum.A4:
            {
                renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
                break;
            }
            default:
            {
                renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
                break;
            }
        }

        using var pdfDocument = renderer.RenderHtmlAsPdf(html);

        using var printDocument = pdfDocument.GetPrintDocument();

        printDocument.PrinterSettings.PrinterName = printerName;
        printDocument.Print();
        }

    private string GetPrinterName(PageSizeEnum pageSize)
    {
        var printerSettingsJson = File.ReadAllText(_printerSettingsPath);
        var printerSettings = JsonSerializer.Deserialize<PrinterSettings>(printerSettingsJson);

        if (printerSettings == null) throw new ArgumentNullException(nameof(printerSettings));

        return pageSize switch
        {
            PageSizeEnum.SixByFour => printerSettings.SixByFourPrinter,
            PageSizeEnum.A4 => printerSettings.A4Printer,
            _ => printerSettings.DefaultPrinter
        };
    }
}