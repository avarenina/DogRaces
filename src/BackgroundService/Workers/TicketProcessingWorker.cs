using Application.Abstractions.BackgroundServices;
using Application.Abstractions.Messaging;
using Application.Tickets.Process;
using Application.Tickets.ProcessTicketBets;
using BackgroundService.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace BackgroundService.Workers;

internal sealed class TicketProcessingWorker(
    IOptions<TicketProcessingConfig> config,
    ILogger<TicketProcessingWorker> logger,
    IServiceScopeFactory scopeFactory)
    : BaseBackgroundService<TicketProcessingConfig>(config)
{
    private readonly ILogger<TicketProcessingWorker> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override ILogger Logger => _logger;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TicketProcessingWorker started at {Time}", DateTime.UtcNow);

        using IServiceScope scope = _scopeFactory.CreateScope();

        ICommandHandler<ProcessTicketsCommand> processTicketsHandler =
            scope.ServiceProvider.GetRequiredService<ICommandHandler<ProcessTicketsCommand>>();

        ICommandHandler<ProcessTicketBetsCommand> processTicketBetsHandler =
           scope.ServiceProvider.GetRequiredService<ICommandHandler<ProcessTicketBetsCommand>>();

        _logger.LogInformation("Starting processing of ticket bets with batch size {BatchSize}", _config.BatchSize);
        Result ticketBetsResult = await processTicketBetsHandler.Handle(new ProcessTicketBetsCommand(_config.BatchSize), cancellationToken);

        if (ticketBetsResult.IsFailure)
        {
            _logger.LogError("Processing ticket bets failed: {Error}", ticketBetsResult.Error.Description);
        }
        else
        {
            _logger.LogInformation("Successfully processed ticket bets.");
        }

        _logger.LogInformation("Starting processing of tickets with batch size {BatchSize}", _config.BatchSize);
        Result ticketsResult = await processTicketsHandler.Handle(new ProcessTicketsCommand(_config.BatchSize), cancellationToken);

        if (ticketsResult.IsFailure)
        {
            _logger.LogError("Processing tickets failed: {Error}", ticketsResult.Error.Description);
        }
        else
        {
            _logger.LogInformation("Successfully processed tickets.");
        }

        _logger.LogInformation("TicketProcessingWorker finished at {Time}", DateTime.UtcNow);
    }
}
