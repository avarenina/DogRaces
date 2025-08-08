using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Races.Finish;

internal sealed class FinishRaceCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IDistributedCache distributedCache,
    IRandomDoubleProvider randomProvider)
    : ICommandHandler<FinishRaceCommand>
{
    public async Task<Result> Handle(FinishRaceCommand command, CancellationToken cancellationToken)
    {
        Race? race = await context.Races
            .Include(r => r.Bets)
            .SingleOrDefaultAsync(r => r.Id == command.RaceId, cancellationToken);

        if (race == null)
        {
            return Result.Failure(RaceErrors.NotFound(command.RaceId));
        }

        try
        {
            race.Finish(dateTimeProvider, randomProvider);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(RaceErrors.AlreadyCompleted(command.RaceId));
        }

        await context.SaveChangesAsync(cancellationToken);

        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        return Result.Success();
    }
}
