using Domain.Bets;
using SharedKernel;

namespace Domain.Tickets;
public class TicketBet : Entity
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public virtual Ticket Ticket { get; set; }
    public virtual Bet Bet { get; set; }
    public decimal Odds { get; set; }
    public BetStatus Status { get; set; }

    public void ResolveStatus()
    {
        if (Status != BetStatus.InProgress)
        {
            return; // already processed
        }

        Status = Bet.Status == BetStatus.Winning
            ? BetStatus.Winning
            : BetStatus.Losing;
    }
}
