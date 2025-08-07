using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Races.Finish;

internal sealed class FinishRaceDomainEventHandler(
    IMessagePublisher messagePublisher,
    ILogger<FinishRaceDomainEventHandler> logger)
    : IDomainEventHandler<RaceFinishedDomainEvent>
{
    public async Task Handle(RaceFinishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var message = new RaceFinishedMessage(domainEvent.RaceId, domainEvent.Result);
            await messagePublisher.PublishAsync(RedisChannels.RaceUpdates, message);

            logger.LogInformation("Published race finished event to Redis for race {RaceId}", domainEvent.RaceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish race finished event to Redis for race {RaceId}", domainEvent.RaceId);
        }
    }
}

public sealed record RaceFinishedMessage(Guid RaceId, List<int> Result);
