using Application.Abstractions.BackgroundServices;

namespace BackgroundService.Configuration;
public class RaceCreationJobConfig: BackgroundServiceConfig
{
   public int TimeBetweenRaces { get; set; }
}
