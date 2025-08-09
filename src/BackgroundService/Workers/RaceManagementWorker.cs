using Application.Abstractions.BackgroundServices;
using Application.Abstractions.Messaging;
using Application.Races.Create;
using Application.Races.Finish;
using Application.Races.Get;
using BackgroundService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace BackgroundService.Workers;

internal sealed class RaceManagementWorker : BaseBackgroundService<RaceManagementConfig>
{
    private readonly ILogger<RaceManagementWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RaceManagementWorker(
        IOptions<RaceManagementConfig> config,
        ILogger<RaceManagementWorker> logger,
        IServiceScopeFactory scopeFactory)
        : base(config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override ILogger Logger => _logger;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ICommandHandler<CreateRaceCommand> createRaceHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateRaceCommand>>();
        ICommandHandler<FinishRaceCommand> finishRaceHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<FinishRaceCommand>>();
        IQueryHandler<GetRacesQuery, List<RaceResponse>> racesQueryHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetRacesQuery, List<RaceResponse>>>();
        IDateTimeProvider dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        _logger.LogInformation("Starting RaceManagementWorker job cycle...");

        List<RaceResponse>? races = await GetUpcomingRacesAsync(racesQueryHandler, cancellationToken);
        if (races == null)
        {
            return;
        }

        if (races.Count < 5)
        {
            races = await CreateRacesIfNeededAsync(races, createRaceHandler, racesQueryHandler, cancellationToken);
        }

        if (races.Count > 0)
        {
            await FinishNextRaceAsync(races[0], finishRaceHandler, dateTimeProvider, cancellationToken);
        }

        _logger.LogInformation("RaceManagementWorker cycle completed.");
    }

    private async Task<List<RaceResponse>?> GetUpcomingRacesAsync(
        IQueryHandler<GetRacesQuery, List<RaceResponse>> racesQueryHandler,
        CancellationToken cancellationToken)
    {
        Result<List<RaceResponse>>? racesResult = await racesQueryHandler.Handle(new GetRacesQuery(IgnoreCache: true), cancellationToken);

        if (racesResult is null || !racesResult.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve upcoming races. Error: {Error}", racesResult?.Error);
            return null;
        }

        _logger.LogInformation("Retrieved {Count} upcoming races.", racesResult.Value.Count);
        return racesResult.Value;
    }

    private async Task<List<RaceResponse>> CreateRacesIfNeededAsync(
        List<RaceResponse> races,
        ICommandHandler<CreateRaceCommand> createRaceHandler,
        IQueryHandler<GetRacesQuery, List<RaceResponse>> racesQueryHandler,
        CancellationToken cancellationToken)
    {
        DateTime? lastRaceStartTime = races.Count > 0 ? races[^1].StartTime : null;

        var createCommand = new CreateRaceCommand
        {
            LastRaceStartTime = lastRaceStartTime,
            AmountOfRacesToCreate = _config.AmountOfRacesToCreate,
            TimeBetweenRaces = _config.TimeBetweenRaces,
            NumberOfRunners = _config.NumberOfRunners,
            BookmakerMargin = _config.BookmakerMargin,
        };

        _logger.LogInformation("Creating new races. LastRaceStartTime: {Time}, Amount: {Amount}, Interval: {Interval}",
            createCommand.LastRaceStartTime, createCommand.AmountOfRacesToCreate, createCommand.TimeBetweenRaces);

        Result createResult = await createRaceHandler.Handle(createCommand, cancellationToken);

        if (!createResult.IsSuccess)
        {
            _logger.LogError("Failed to create races: {Error}", createResult.Error);
            return races;
        }

        _logger.LogInformation("Successfully created {Amount} races.", createCommand.AmountOfRacesToCreate);

        // If there were no races before, re-fetch to get the newly created ones
        if (races.Count == 0)
        {
            List<RaceResponse>? updatedRaces = await GetUpcomingRacesAsync(racesQueryHandler, cancellationToken);
            if (updatedRaces != null)
            {
                return updatedRaces;
            }
        }

        return races;
    }

    private async Task FinishNextRaceAsync(
        RaceResponse nextRace,
        ICommandHandler<FinishRaceCommand> finishRaceHandler,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken)
    {
        TimeSpan delay = nextRace.StartTime.AddSeconds(-_config.FinishRaceOffset) - dateTimeProvider.UtcNow;

        if (delay > TimeSpan.Zero)
        {
            _logger.LogInformation("Waiting {Delay} until next race (ID: {RaceId}) starts.", delay, nextRace.Id);
            await Task.Delay(delay, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Next race (ID: {RaceId}) is scheduled in the past. Proceeding immediately.", nextRace.Id);
        }

        var finishCommand = new FinishRaceCommand(nextRace.Id);
        Result finishResult = await finishRaceHandler.Handle(finishCommand, cancellationToken);

        if (finishResult.IsSuccess)
        {
            _logger.LogInformation("Successfully finished race (ID: {RaceId}) at {Time}.", nextRace.Id, dateTimeProvider.UtcNow);
        }
        else
        {
            _logger.LogError("Failed to finish race (ID: {RaceId}): {Error}", nextRace.Id, finishResult.Error);
        }
    }
}
