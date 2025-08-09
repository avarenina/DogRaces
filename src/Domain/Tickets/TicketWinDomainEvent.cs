using SharedKernel;

namespace Domain.Tickets;
public sealed record TicketWinDomainEvent(Guid TicketId, decimal WinAmount) : IDomainEvent;
