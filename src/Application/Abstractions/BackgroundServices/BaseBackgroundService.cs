using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Abstractions.BackgroundServices;

public abstract class BaseBackgroundService<TConfig> : BackgroundService
    where TConfig : BackgroundServiceConfig
{
    protected readonly TConfig _config;

    protected BaseBackgroundService(IOptions<TConfig> config)
    {
        _config = config.Value;
    }

    protected abstract Task ExecuteJobAsync(CancellationToken cancellationToken);
    protected abstract ILogger Logger { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.Enabled)
        {
            Logger.LogInformation("{ServiceName} is disabled.", GetType().Name);
            return;
        }

        Logger.LogInformation("{ServiceName} started with interval {IntervalSeconds}s",
            GetType().Name, _config.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteJobAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{ServiceName} failed.", GetType().Name);
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.IntervalSeconds), stoppingToken);
        }
    }
}
