using Azure.Messaging.ServiceBus;
using PdfClient.Services;
using PdfClient.Services.Interfaces;

namespace PdfClient;

public class Executor
{
    private readonly IPdfMessagePrinter _pdfMessagePrinter;
    private readonly ServiceBusClient _serviceBusClient;

    public Executor(ServiceBusClient serviceBusClient, IPdfMessagePrinter pdfMessagePrinter)
    {
        _serviceBusClient = serviceBusClient;
        _pdfMessagePrinter = pdfMessagePrinter;
    }

    public async Task Execute()
    {
        var processor = _serviceBusClient.CreateProcessor("pdfqueue");

        processor.ProcessMessageAsync += async args =>
        {
            var printer = args.Message.ApplicationProperties["pageSize"].ToString();
            var stream = args.Message.Body.ToStream();
            using var streamReader = new StreamReader(stream);
            var html = await streamReader.ReadToEndAsync();

            if (Enum.TryParse(printer, out PageSizeEnum pageSize))
            {
                _pdfMessagePrinter.CreateAndPrintPdf(html, pageSize);
            }
            else
            {
                Console.WriteLine($"Invalid printer value: {printer}");
            }

            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
            return Task.CompletedTask;
        };

        try
        {
            await processor.StartProcessingAsync();
            Console.WriteLine("Connected to the Service Bus Queue.");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await processor.StopProcessingAsync();
        }
    }
}
