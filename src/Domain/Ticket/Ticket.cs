using Domain.Bets;
using SharedKernel;

namespace Domain.Ticket;
public class Ticket : Entity
{
    public Guid Id { get; set; }
    public TicketStatus Status { get; set; }
    public virtual ICollection<TicketBet> Bets { get; set; } = [];
    public decimal Payin { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal WinAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool IsCompleted => Bets.All(b => b.Status != BetStatus.InProgress);

    public bool IsWinning()
    {
        return Bets.All(b => b.Status == BetStatus.Winning);
    }

    public void Complete(DateTime completedAt)
    {
        if (!IsCompleted)
        {
            throw new InvalidOperationException("Ticket is not yet complete.");
        }

        if (IsWinning())
        {
            Status = TicketStatus.Won;
            Raise(new TicketWinDomainEvent(Id, WinAmount));
        }
        else
        {
            Status = TicketStatus.Lost;
        }

        CompletedAt = completedAt;
    }
}

