using SharedKernel;

namespace Domain.Ticket;

public sealed record TicketCreatedDomainEvent(Guid TicketId) : IDomainEvent;


