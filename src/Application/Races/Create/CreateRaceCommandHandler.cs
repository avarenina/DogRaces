using System.Collections.Generic;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Races.Create;

internal sealed class CreateRaceCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IDistributedCache distributedCache)
    : ICommandHandler<CreateRaceCommand, List<Guid>>
{
    public async Task<Result<List<Guid>>> Handle(CreateRaceCommand command, CancellationToken cancellationToken)
    {
        DateTime startTime = command.LastRaceStartTime ?? dateTimeProvider.UtcNow;

        var response = new List<Guid> ();

        for (int i = 0; i < command.AmountOfRacesToCreate - 1; i++)
        {
            // first we need to update last start time
            startTime = startTime.AddSeconds(command.TimeBetweenRaces);

            var race = new Race
            {
                Id = Guid.NewGuid(),
                StartTime = startTime,
                CreatedAt = dateTimeProvider.UtcNow,
                Status = RaceStatus.Open
            };

            // we are going to rase the event with new races so that we can notify services
            race.Raise(new RaceCreatedDomainEvent(race.Id));

            context.Races.Add(race);
            response.Add(race.Id);
        }

        await context.SaveChangesAsync(cancellationToken);

        // invalidate cache
        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        return response;
    }
}
