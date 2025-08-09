using Domain.Bets;
using SharedKernel;

namespace Domain.Tickets;
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

    public bool Validate(
        TicketValidationOptions options,
        out Error? error)
    {
        error = null;
        var bets = Bets.Select(tb => tb.Bet).ToList();

        // one bet per race
        var distinctRaceIds = bets.Select(b => b.Race.Id).Distinct().ToList();
        if (bets.Count != distinctRaceIds.Count)
        {
            error = TicketErrors.OneBetPerRaceAllowed();
            return false;
        }

        decimal totalOdds = bets.Aggregate(1m, (acc, b) => acc * b.Odds);
        if (totalOdds < options.MinTotalOdds || totalOdds > options.MaxTotalOdds)
        {
            error = TicketErrors.TotalOddsOutOfRange(options.MinTotalOdds, options.MaxTotalOdds);
            return false;
        }

        decimal winAmount = Math.Round(Payin * totalOdds, 2, MidpointRounding.AwayFromZero);
        if (winAmount > options.MaxWin)
        {
            error = TicketErrors.MaxWinExceeded(options.MaxWin);
            return false;
        }

        return true;
    }
}


