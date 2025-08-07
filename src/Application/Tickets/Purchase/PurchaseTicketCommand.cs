using Application.Abstractions.Messaging;

namespace Application.Tickets.Purchase;

public sealed class PurchaseTicketCommand : ICommand
{
    public Guid Id { get; set; }
    public decimal Payin {  get; set; }
    public List<Guid> Bets { get; set; }
}
