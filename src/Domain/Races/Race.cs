using Domain.Abstractions;
using Domain.Bets;
using SharedKernel;

namespace Domain.Races;

public class Race : Entity
{
    protected Race() { }

    public Race(
    Guid id,
    double[] probabilities,
    DateTime startTime,
    DateTime createdAt,
    RaceStatus status)
    {
        Id = id;
        Probabilities = probabilities;
        StartTime = startTime;
        CreatedAt = createdAt;
        Status = status;
        Bets = [];
    }

    public Guid Id { get; set; }
    public string? Result { get; set; }
    public virtual ICollection<Bet> Bets { get; set; } = [];
    public bool IsCompleted { get; set; }
    public double[] Probabilities { get; set; } = [];
    public DateTime StartTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RaceStatus Status { get; set; }
  
    public void Finish(IDateTimeProvider dateTimeProvider, IRandomDoubleProvider randomProvider)
    {
        if (IsCompleted || !string.IsNullOrEmpty(Result))
        {
            throw new InvalidOperationException("Race is already completed.");
        }

        List<int> raceResult = SimulateRaceResult(randomProvider);

        Result = string.Join(",", raceResult);
        IsCompleted = true;
        CompletedAt = dateTimeProvider.UtcNow;

        foreach (Bet bet in Bets)
        {
            bet.UpdateStatus(raceResult);
        }

        Raise(new RaceFinishedDomainEvent(Id, raceResult));
    }

    private List<int> SimulateRaceResult(IRandomDoubleProvider randomProvider)
    {
        var result = new List<int>();
        var remaining = Enumerable.Range(1, Probabilities.Length).ToList();
        double[] remainingProbabilities = (double[])Probabilities.Clone();

        while (remaining.Count > 0)
        {
            double total = remaining.Sum(r => remainingProbabilities[r - 1]);
            if (total <= 0.0)
            {
                int fallbackIndex = (int)(randomProvider.NextDouble() * remaining.Count);
                result.Add(remaining[fallbackIndex]);
                remaining.RemoveAt(fallbackIndex);
                continue;
            }

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
                result.Add(remaining[^1]);
                remaining.RemoveAt(remaining.Count - 1);
            }
        }

        return result;
    }
}
