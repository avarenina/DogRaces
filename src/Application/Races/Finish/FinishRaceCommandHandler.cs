using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Races.Finish;

internal sealed class FinishRaceCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IDistributedCache distributedCache)
    : ICommandHandler<FinishRaceCommand>
{
    public async Task<Result> Handle(FinishRaceCommand command, CancellationToken cancellationToken)
    {
        Race? race = await context.Races
            .SingleOrDefaultAsync(t => t.Id == command.RaceId, cancellationToken);

        if (race is null)
        {
            return Result.Failure(RaceErrors.NotFound(command.RaceId));
        }

        if (race.IsCompleted)
        {
            return Result.Failure(RaceErrors.AlreadyCompleted(command.RaceId));
        }
        race.Result = command.Result;
        race.IsCompleted = true;
        race.CompletedAt = dateTimeProvider.UtcNow;

        race.Raise(new RaceFinishedDomainEvent(race.Id));

        await context.SaveChangesAsync(cancellationToken);

        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        return Result.Success();
    }
}
