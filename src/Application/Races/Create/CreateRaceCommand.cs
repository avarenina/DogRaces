using Application.Abstractions.Messaging;
using Domain.Races;

namespace Application.Races.Create;

public sealed class CreateRaceCommand : ICommand<List<Guid>>
{
    public DateTime? LastRaceStartTime { get; set; }
    public int AmountOfRacesToCreate { get; set; }
    public int TimeBetweenRaces { get; set; }  
    public int NumberOfRunners { get; set; }
    public double BookmakerMargin { get; set; }
}
