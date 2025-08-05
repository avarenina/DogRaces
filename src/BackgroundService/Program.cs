using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BackgroundService.Configuration;
using BackgroundService.Jobs;

namespace BackgroundService;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        // Create the host
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Bind configuration section to strongly typed config class
                services.Configure<RaceCreationJobConfig>(
                    context.Configuration.GetSection("RaceCreationJob"));

                // Register hosted service
                services.AddHostedService<RaceCreationJob>();

                // Optional: Register other application or infrastructure services if needed
                // services.AddApplicationServices();
                // services.AddInfrastructureServices(context.Configuration);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        // Run the host
        await host.RunAsync();
    }
}
