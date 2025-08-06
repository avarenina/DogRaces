using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BackgroundService.Configuration;
using BackgroundService.Jobs;
using Application;
using Infrastructure;
using SharedKernel;
using Infrastructure.Time;

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

                // Register application services
                services.AddApplication();
                
                // Register infrastructure services
                services.AddInfrastructure(context.Configuration);
                services.AddMessagePublishers();

                // Register hosted service
                services.AddHostedService<RaceCreationJob>();
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
