using System.Collections.Generic;
using Application.Abstractions;
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
    IDistributedCache distributedCache,
    IRaceFactory raceFactory)
    : ICommandHandler<CreateRaceCommand, List<Guid>>
{
    public async Task<Result<List<Guid>>> Handle(CreateRaceCommand command, CancellationToken cancellationToken)
    {
        DateTime startTime = command.LastRaceStartTime ?? dateTimeProvider.UtcNow;

        var response = new List<Guid> ();

        for (int i = 0; i < command.AmountOfRacesToCreate; i++)
        {
            // first we need to update last start time
            startTime = startTime.AddSeconds(command.TimeBetweenRaces);

            Race race = raceFactory.Create(startTime, command.NumberOfRunners, command.BookmakerMargin);

            context.Races.Add(race);
            response.Add(race.Id);
        }

        await context.SaveChangesAsync(cancellationToken);

        // invalidate cache
        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        return response;
    }
}
