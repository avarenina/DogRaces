using Domain.Abstractions;
using Domain.Races;
using SharedKernel;

namespace Domain.Factories;
public sealed class RaceBatchFactory : IRaceBatchFactory
{
    private readonly IRaceFactory _raceFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RaceBatchFactory(IRaceFactory raceFactory, IDateTimeProvider dateTimeProvider)
    {
        _raceFactory = raceFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public IReadOnlyList<Race> CreateBatch(
        DateTime? lastRaceStartTime,
        int amountOfRacesToCreate,
        int numberOfRunners,
        double bookmakerMargin,
        int timeBetweenSeconds)
    {
        var races = new List<Race>();
        DateTime startTime = lastRaceStartTime ?? _dateTimeProvider.UtcNow;

        for (int i = 0; i < amountOfRacesToCreate; i++)
        {
            startTime = startTime.AddSeconds(timeBetweenSeconds);
            Race race = _raceFactory.Create(startTime, numberOfRunners, bookmakerMargin);
            races.Add(race);
        }

        return races;
    }
}
