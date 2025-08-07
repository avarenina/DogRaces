using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Races.Finish;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application.Abstractions;

namespace Infrastructure.Messaging.Handlers;
public sealed class RaceFinishedMessageHandler : IMessageHandler<RaceFinishedMessage>
{
    private readonly IClientsNotifier _racesNotifier;
    private readonly ILogger<RaceFinishedMessageHandler> _logger;

    public RaceFinishedMessageHandler(IClientsNotifier racesNotifier, ILogger<RaceFinishedMessageHandler> logger)
    {
        _racesNotifier = racesNotifier;
        _logger = logger;
    }

    public async Task HandleAsync(RaceFinishedMessage message)
    {
        try
        {
            await _racesNotifier.NotifyRaceFinishedAsync(message);
            _logger.LogInformation("Successfully triggered races update for finished race {RaceId}", message.RaceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling race finished message for race {RaceId}", message.RaceId);
        }
    }
}
