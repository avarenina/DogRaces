using Application.Races.Finish;

namespace Application.Abstractions;

public interface IClientsNotifier
{
    Task NotifyNewUpcomingRacesAsync(CancellationToken cancellationToken = default);
    Task NotifyRaceFinishedAsync(RaceFinishedMessage message, CancellationToken cancellationToken = default);
}
