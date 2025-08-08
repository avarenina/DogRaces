using SharedKernel;

namespace Domain.Ticket;
public sealed record TicketWonDomainEvent(Guid TicketId, decimal Amount) : IDomainEvent;
