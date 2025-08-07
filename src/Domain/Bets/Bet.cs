using Domain.Races;
using Domain.Ticket;
using SharedKernel;

namespace Domain.Bets;

public abstract class Bet : Entity
{
    public Guid Id { get; set; }
    public decimal Odds { get; set; }
    public virtual Race Race { get; set; }
    public virtual ICollection<TicketBet> TicketBets { get; set; } = [];
    public List<int> Runners { get; set; }
    public BetStatus Status { get; set; }
    public BetType Type { get; set; }
    public abstract BetStatus GetStatusOnRaceEnd(List<int> raceResult);
}
