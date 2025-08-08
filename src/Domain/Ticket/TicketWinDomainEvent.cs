using SharedKernel;

namespace Domain.Ticket;
public sealed record TicketWinDomainEvent(Guid TicketId, decimal WinAmount) : IDomainEvent;
