using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Races.Create;

internal sealed class RaceCreatedDomainEventHandler(
    IMessagePublisher messagePublisher,
    ILogger<RaceCreatedDomainEventHandler> logger)
    : IDomainEventHandler<RaceCreatedDomainEvent>
{
    public async Task Handle(RaceCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var message = new RaceCreatedMessage(domainEvent.RaceId);
            await messagePublisher.PublishAsync(RedisChannels.RaceUpdates, message);
            
            logger.LogInformation("Published race created event to Redis for race {RaceId}", domainEvent.RaceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish race created event to Redis for race {RaceId}", domainEvent.RaceId);
        }
    }
}

public sealed record RaceCreatedMessage(Guid RaceId); 