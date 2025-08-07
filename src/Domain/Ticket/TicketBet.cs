using Domain.Bets;
using SharedKernel;

namespace Domain.Ticket;
public class TicketBet : Entity
{
    public Guid Id { get; set; }
    public virtual Ticket Ticket { get; set; }
    public virtual Bet Bet { get; set; }
    public decimal Odds { get; set; }
    public BetStatus Status { get; set; }
}
