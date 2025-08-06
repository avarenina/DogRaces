using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Web.Api.Hubs;

namespace Web.Api.Services;

internal sealed class RaceCreatedMessageHandler : IMessageHandler<RaceCreatedMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RaceCreatedMessageHandler> _logger;

    public RaceCreatedMessageHandler(
        IServiceProvider serviceProvider,
        ILogger<RaceCreatedMessageHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync(RaceCreatedMessage message)
    {
        try
        {
            _logger.LogInformation("Received race created message for race {RaceId}", message.RaceId);
            
            // Trigger immediate update to all connected clients
            using IServiceScope scope = _serviceProvider.CreateScope();
            RacesUpdateService racesUpdateService = scope.ServiceProvider.GetRequiredService<RacesUpdateService>();
            
            await racesUpdateService.SendRacesUpdate();
            
            _logger.LogInformation("Successfully triggered races update for new race {RaceId}", message.RaceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling race created message for race {RaceId}", message.RaceId);
        }
    }
}

public sealed record RaceCreatedMessage(Guid RaceId); 