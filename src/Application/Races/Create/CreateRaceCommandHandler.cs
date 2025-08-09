using System.Collections.Generic;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Factories;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Races.Create;

internal sealed class CreateRaceCommandHandler(
    IApplicationDbContext context,
    IDistributedCache distributedCache,
    IRaceBatchFactory raceBatchFactory,
    IMessagePublisher messagePublisher)
    : ICommandHandler<CreateRaceCommand>
{
    public async Task<Result> Handle(CreateRaceCommand command, CancellationToken cancellationToken)
    {
        
        IReadOnlyList<Race> races = raceBatchFactory.CreateBatch(
            command.LastRaceStartTime,
            command.AmountOfRacesToCreate,
            command.NumberOfRunners,
            command.BookmakerMargin,
            command.TimeBetweenRaces);

        context.Races.AddRange(races);

        await context.SaveChangesAsync(cancellationToken);

        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        // TODO : Raise batch event
        var message = new RaceCreatedMessage();
        await messagePublisher.PublishAsync(RedisChannels.RaceUpdates, message);

        return Result.Success();
    }
}
public sealed record RaceCreatedMessage();
