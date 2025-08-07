using Domain.Bets;
using Domain.Ticket;

namespace Application.Abstractions;

public interface ITicketFactory
{
    Ticket Create(Guid id, IReadOnlyCollection<Bet> bets, decimal payin);
}


