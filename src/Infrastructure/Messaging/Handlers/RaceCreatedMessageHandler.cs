using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Races.Create;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Abstractions;

namespace Infrastructure.Messaging.Handlers;
public sealed class RaceCreatedMessageHandler : IMessageHandler<RaceCreatedMessage>
{
    private readonly IClientsNotifier _racesNotifier;
    private readonly ILogger<RaceCreatedMessageHandler> _logger;

    public RaceCreatedMessageHandler(IClientsNotifier racesNotifier, ILogger<RaceCreatedMessageHandler> logger)
    {
        _racesNotifier = racesNotifier;
        _logger = logger;
    }

    public async Task HandleAsync(RaceCreatedMessage message)
    {
        try
        {
            _logger.LogInformation("Received race created message");

            await _racesNotifier.NotifyNewUpcomingRacesAsync();

            _logger.LogInformation("Successfully triggered races update for new race");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling race created message");
        }
    }
}
