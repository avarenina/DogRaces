using Domain.Races;

namespace Domain.Abstractions;
public interface IRaceFactory
{
    Race Create(DateTime startTime, int numberOfRunners, double bookmakerMargin);
}
