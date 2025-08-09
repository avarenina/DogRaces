using Application.Abstractions;
using Domain.Tickets;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Tickets.Purchase;

internal sealed class TicketPurchaseEventHandler(
    IClientsNotifier clientsNotifier,
    ILogger<TicketPurchaseEventHandler> logger)
    : IDomainEventHandler<TicketPurchaseDomainEvent>
{
    public async Task Handle(TicketPurchaseDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling TicketCreatedDomainEvent: TicketId = {TicketId}, Amount = {Amount}",
            domainEvent.TicketId, domainEvent.Amount);

        try
        {
            await clientsNotifier.NotifyBalanceChangedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while handling TicketCreatedDomainEvent for TicketId = {TicketId}",
                domainEvent.TicketId);
        }
    }
}
