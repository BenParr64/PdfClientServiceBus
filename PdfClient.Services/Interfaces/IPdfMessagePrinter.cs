namespace PdfClient.Services.Interfaces
{
    public interface IPdfMessagePrinter
    {
        void CreateAndPrintPdf(string html, PageSizeEnum pageSize);
    }
}
