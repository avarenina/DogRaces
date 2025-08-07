using Application.Abstractions.Messaging;
using Domain.Races;

namespace Application.Races.Create;

public sealed record CreateRaceCommand : ICommand
{
    public DateTime? LastRaceStartTime { get; init; }
    public int AmountOfRacesToCreate { get; init; }
    public int TimeBetweenRaces { get; init; }  
    public int NumberOfRunners { get; init; }
    public double BookmakerMargin { get; init; }
}
