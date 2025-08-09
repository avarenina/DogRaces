using Domain.Abstractions;
using Domain.Races;
using SharedKernel;

namespace Domain.Factories;

public class RaceFactory(
    IDateTimeProvider dateTimeProvider,
    IProbabilityCalculator probabilityCalculator,
    IBetFactory betFactory
): IRaceFactory
{
    public Race Create(DateTime startTime, int numberOfRunners, double bookmakerMargin)
    {
        var id = Guid.NewGuid();
        double[] probabilities = probabilityCalculator.CalculateWinnerProbabilities(numberOfRunners, bookmakerMargin);
        DateTime createdAt = dateTimeProvider.UtcNow;

        var race = new Race(id, probabilities, startTime, createdAt, RaceStatus.Open);

        race.Bets = betFactory.Create(race);

        race.Raise(new RaceCreatedDomainEvent(race.Id));

        return race;
    }
}
