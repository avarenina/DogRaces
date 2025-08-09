using Domain.Bets;
using Domain.Tickets;

namespace Domain.Abstractions;

public interface ITicketFactory
{
    Ticket Create(Guid id, IReadOnlyCollection<Bet> bets, decimal payin);
}


