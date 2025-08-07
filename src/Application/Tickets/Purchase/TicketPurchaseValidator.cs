using Domain.Bets;
using SharedKernel;

namespace Application.Tickets.Purchase;

internal sealed class TicketPurchaseValidator : ITicketPurchaseValidator
{
    public bool Validate(
        PurchaseTicketCommand command,
        IReadOnlyCollection<Bet> bets,
        TicketValidationOptions options,
        out Error? error)
    {
        error = null;

        // one bet per race
        var distinctRaceIds = bets.Select(b => b.Race.Id).Distinct().ToList();
        if (bets.Count != distinctRaceIds.Count)
        {
            error = Domain.Ticket.TicketErrors.OneBetPerRaceAllowed();
            return false;
        }

        decimal totalOdds = bets.Aggregate(1m, (acc, b) => acc * b.Odds);
        if (totalOdds < options.MinTotalOdds || totalOdds > options.MaxTotalOdds)
        {
            error = Domain.Ticket.TicketErrors.TotalOddsOutOfRange(options.MinTotalOdds, options.MaxTotalOdds);
            return false;
        }

        decimal winAmount = Math.Round(command.Payin * totalOdds, 2, MidpointRounding.AwayFromZero);
        if (winAmount > options.MaxWin)
        {
            error = Domain.Ticket.TicketErrors.MaxWinExceeded(options.MaxWin);
            return false;
        }

        return true;
    }
}


