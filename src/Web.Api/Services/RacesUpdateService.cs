using Application.Abstractions.Messaging;
using Application.Races.Get;
using Microsoft.AspNetCore.SignalR;
using Web.Api.Hubs;

namespace Web.Api.Services;

public sealed class RacesUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RacesUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    public RacesUpdateService(IServiceProvider serviceProvider, ILogger<RacesUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRacesUpdate();
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending races update");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public async Task SendRacesUpdate()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IHubContext<RacesHub> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<RacesHub>>();
        IQueryHandler<GetRacesQuery, List<RaceResponse>> queryHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRacesQuery, List<RaceResponse>>>();

        var query = new GetRacesQuery();
        SharedKernel.Result<List<RaceResponse>> result = await queryHandler.Handle(query, CancellationToken.None);

        if (result.IsSuccess)
        {
            await hubContext.Clients.Group("RacesGroup").SendAsync("ReceiveUpcomingRaces", result.Value);
            _logger.LogInformation("Sent {Count} upcoming races to connected clients", result.Value.Count);
        }
        else
        {
            _logger.LogWarning("Failed to get races for update: {Error}", result.Error);
        }
    }
} 
