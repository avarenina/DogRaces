using Application.Abstractions.Messaging;

namespace Application.Tickets.Purchase;

public sealed record PurchaseTicketCommand : ICommand
{
    public Guid Id { get; init; }
    public decimal Payin { get; init; }
    public List<Guid> Bets { get; init; }
}
