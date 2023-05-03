using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using PdfClient.Services;
using PdfClient.Services.Interfaces;

namespace PdfClient;

public class Program
{
    static void Main(string[] args)
    {
        var printerSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "printer-settings.json");

        // Set up the dependency injection container
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<IPdfMessagePrinter>(provider =>
            new PdfMessagePrinter(printerSettingsPath));
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient("");
        });

        services
            .AddSingleton<Executor, Executor>()
            .BuildServiceProvider()
            .GetService<Executor>()!
            .Execute();
    }
}