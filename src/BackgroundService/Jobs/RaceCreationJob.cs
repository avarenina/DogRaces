using Application.Abstractions.BackgroundServices;
using Application.Abstractions.Messaging;
using Application.Races.Create;
using Application.Races.Get;
using BackgroundService.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace BackgroundService.Jobs;
internal sealed class RaceCreationJob : BaseBackgroundService<RaceCreationJobConfig>
{
    private readonly ILogger<RaceCreationJob> _logger;
    private readonly ICommandHandler<CreateRaceCommand, List<Guid>> _createRaceHandler;
    private readonly IQueryHandler<GetRacesQuery, List<RaceResponse>> _racesQueryHandler;

    public RaceCreationJob(
        IOptions<RaceCreationJobConfig> config,
        ILogger<RaceCreationJob> logger,
        ICommandHandler<CreateRaceCommand, List<Guid>> createRaceHandler,
        IQueryHandler<GetRacesQuery, List<RaceResponse>> racesQueryHandler)
        : base(config)
    {
        _logger = logger;
        _createRaceHandler = createRaceHandler;
        _racesQueryHandler = racesQueryHandler;

    }

    protected override ILogger Logger => _logger;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        try
        {
            Result<List<RaceResponse>> upcomingRacesResult = await _racesQueryHandler.Handle(new GetRacesQuery(IgnoreCache: true), cancellationToken);

            if (upcomingRacesResult == null)
            {
                _logger.LogWarning("GetRacesQuery returned null result.");
                return;
            }

            if (!upcomingRacesResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve upcoming races: {Error}", upcomingRacesResult.Error);
                return;
            }

            List<RaceResponse> races = upcomingRacesResult.Value;
            _logger.LogInformation("Retrieved {RaceCount} upcoming races.", races.Count);

            if (races.Count < 5)
            {
                _logger.LogInformation("Less than 5 races found. Proceeding to create new races.");

                DateTime? lastRaceTime = races.Count > 0 ? races[^1].StartTime : null;

                var command = new CreateRaceCommand
                {
                    LastRaceStartTime = lastRaceTime,
                    AmountOfRacesToCreate = _config.AmountOfRacesToCreate,
                    TimeBetweenRaces = _config.TimeBetweenRaces,
                    NumberOfRunners = _config.NumberOfRunners,
                    BookmakerMargin = _config.BookmakerMargin,
                };

                _logger.LogInformation("Sending CreateRaceCommand with LastRaceStartTime: {LastRaceTime}, Amount: {Amount}, Interval: {Interval}",
                    command.LastRaceStartTime, command.AmountOfRacesToCreate, command.TimeBetweenRaces);

                Result result = await _createRaceHandler.Handle(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully created {Amount} races starting from {StartTime}.",
                        command.AmountOfRacesToCreate, command.LastRaceStartTime);
                }
                else
                {
                    _logger.LogError("Failed to create races: {Error}", result.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while checking or creating races.");
        }
    }
}
