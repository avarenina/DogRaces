using SharedKernel;

namespace Domain.Tickets;

public sealed record TicketPurchaseDomainEvent(Guid TicketId, decimal Amount) : IDomainEvent;


