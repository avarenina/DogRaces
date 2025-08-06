using Domain.Races;

namespace Application.Abstractions;
public interface IRaceFactory
{
    Race Create(DateTime startTime, int numberOfRunners, double bookmakerMargin);
}
