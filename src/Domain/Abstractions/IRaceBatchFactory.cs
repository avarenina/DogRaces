using Domain.Races;

namespace Domain.Abstractions;
public interface IRaceBatchFactory
{
    IReadOnlyList<Race> CreateBatch(
        DateTime? lastRaceStartTime,
        int amountOfRacesToCreate,
        int numberOfRunners,
        double bookmakerMargin,
        int timeBetweenSeconds);
}
