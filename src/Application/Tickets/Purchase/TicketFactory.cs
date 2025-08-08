using Application.Abstractions;
using Domain.Bets;
using Domain.Ticket;
using SharedKernel;

namespace Application.Tickets.Purchase;

public sealed class TicketFactory(IDateTimeProvider dateTimeProvider) : ITicketFactory
{
    public Ticket Create(Guid id, IReadOnlyCollection<Bet> bets, decimal payin)
    {
        decimal totalOdds = bets.Aggregate(1m, (acc, b) => acc * b.Odds);
        decimal winAmount = Math.Round(payin * totalOdds, 2, MidpointRounding.AwayFromZero);

        var ticket = new Ticket
        {
            Id = id,
            Status = TicketStatus.Pending,
            Bets = [.. bets.Select(b => new TicketBet
            {
                Bet = b,
                Odds = b.Odds,
                Status = BetStatus.InProgress,
            })],
            Payin = payin,
            TotalOdds = totalOdds,
            WinAmount = winAmount,
            CreatedAt = dateTimeProvider.UtcNow,
        };

        return ticket;
    }
}


