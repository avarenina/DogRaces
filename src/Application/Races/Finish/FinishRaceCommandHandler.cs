using Application.Abstractions;
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
    IDistributedCache distributedCache,
    IRandomDoubleProvider randomProvider)
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

        if (race.IsCompleted || !string.IsNullOrEmpty(race.Result))
        {
            return Result.Failure(RaceErrors.AlreadyCompleted(command.RaceId));
        }

        List<int> result = SimulateRaceResult(race);

        race.Result = string.Join(",", result);

        race.IsCompleted = true;
        race.CompletedAt = dateTimeProvider.UtcNow;

        race.Raise(new RaceFinishedDomainEvent(race.Id));

        await context.SaveChangesAsync(cancellationToken);

        await distributedCache.RemoveAsync(CacheKeys.UpcomingRaces, cancellationToken);

        return Result.Success();
    }


    private List<int> SimulateRaceResult(Race race)
    {
        var result = new List<int>();
        var remaining = Enumerable.Range(1, race.Probabilities.Length).ToList(); // 1-based runner numbers
        double[] remainingProbabilities = (double[])race.Probabilities.Clone();

        while (remaining.Count > 0)
        {
            // Normalize current probabilities
            double total = remaining.Sum(r => remainingProbabilities[r - 1]);

            var normalized = remaining
                .Select(r => new { Runner = r, Prob = remainingProbabilities[r - 1] / total })
                .ToList();

            // Roll to select next position
            double roll = randomProvider.NextDouble();
            double cumulative = 0;

            foreach (var item in normalized)
            {
                cumulative += item.Prob;
                if (roll <= cumulative)
                {
                    result.Add(item.Runner);
                    remaining.Remove(item.Runner);
                    break;
                }
            }
        }

        return result;
    }
}
