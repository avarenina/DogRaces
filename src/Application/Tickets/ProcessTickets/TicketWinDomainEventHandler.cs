using Application.Abstractions.Messaging;
using Domain.Races;
using Domain.Ticket;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Tickets.ProcessTickets;

internal sealed class TicketWinDomainEventHandler(
    IMessagePublisher messagePublisher,
    ILogger<TicketWinDomainEventHandler> logger)
    : IDomainEventHandler<TicketWinDomainEvent>
{
    public async Task Handle(TicketWinDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TicketWinDomainEvent: TicketId = {TicketId}, WinAmount = {WinAmount}",
            domainEvent.TicketId, domainEvent.WinAmount);

        try
        {
            var message = new TicketWinMessage(domainEvent.TicketId, domainEvent.WinAmount);
            await messagePublisher.PublishAsync(RedisChannels.TicketUpdates, message);

            logger.LogInformation("Published TicketWinMessage for TicketId = {TicketId}", domainEvent.TicketId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while handling TicketWinDomainEvent for TicketId = {TicketId}", domainEvent.TicketId);
        }
    }
}

public sealed record TicketWinMessage(Guid TicketId, decimal WinAmount);
