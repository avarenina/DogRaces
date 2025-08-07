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
            .Include(r => r.Bets)
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

        // since we have a relatively small set of bets this will work fine. However, for larger sets use something like bulk update using Table-Valued Parameters (TVPs)
        foreach (Domain.Bets.Bet bet in race.Bets)
        {
            bet.Status = bet.GetStatusOnRaceEnd(result);
        }

        race.Raise(new RaceFinishedDomainEvent(race.Id, result));

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
            // Sum current remaining probabilities
            double total = remaining.Sum(r => remainingProbabilities[r - 1]);

            if (total <= 0.0)
            {
                // Fallback: assign equal probability if all remaining runners have zero probability
                int fallbackIndex = (int)(randomProvider.NextDouble() * remaining.Count);
                result.Add(remaining[fallbackIndex]);
                remaining.RemoveAt(fallbackIndex);
                continue;
            }

            // Normalize probabilities
            var normalized = remaining
                .Select(r => new { Runner = r, Prob = remainingProbabilities[r - 1] / total })
                .ToList();

            double roll = randomProvider.NextDouble();
            double cumulative = 0.0;
            bool selected = false;

            foreach (var item in normalized)
            {
                cumulative += item.Prob;
                if (roll < cumulative)
                {
                    result.Add(item.Runner);
                    remaining.Remove(item.Runner);
                    selected = true;
                    break;
                }
            }

            if (!selected)
            {
                // Fallback: due to precision issues, select the last remaining runner
                result.Add(remaining[^1]);
                remaining.RemoveAt(remaining.Count - 1);
            }
        }

        return result;
    }
}
