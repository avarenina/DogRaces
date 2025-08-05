using Application.Abstractions.BackgroundServices;
using BackgroundService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundService.Jobs;
internal sealed class RaceCreationJob : BaseBackgroundService<RaceCreationJobConfig>
{
    private readonly ILogger<RaceCreationJob> _logger;

    public RaceCreationJob(
        IOptions<RaceCreationJobConfig> config,
        ILogger<RaceCreationJob> logger)
        : base(config)
    {
        _logger = logger;
    }

    protected override ILogger Logger => _logger;

    protected override Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        // Your job logic here
        return Task.CompletedTask;
    }
}
