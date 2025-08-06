using Application.Abstractions.BackgroundServices;

namespace BackgroundService.Configuration;
public class RaceCreationJobConfig : BackgroundServiceConfig
{
    public int AmountOfRacesToCreate { get; set; }
    public int TimeBetweenRaces { get; set; }
    public int NumberOfRunners { get; set; }
    public double BookmakerMargin { get; set; }
}
