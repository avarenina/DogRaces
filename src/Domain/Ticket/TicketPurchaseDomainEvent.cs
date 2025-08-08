using SharedKernel;

namespace Domain.Ticket;

public sealed record TicketPurchaseDomainEvent(Guid TicketId, decimal Amount) : IDomainEvent;


