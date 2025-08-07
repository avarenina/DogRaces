using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Races.Get;
using Domain.Bets;
using Domain.Races;
using Domain.Ticket;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tickets.Purchase;
internal sealed class PurchaseTicketCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider) 
    : ICommandHandler<PurchaseTicketCommand>
{
    public async Task<Result> Handle(PurchaseTicketCommand command, CancellationToken cancellationToken)
    {

        List<Bet> bets = await context.Bets
            .Include(b => b.Race)
            .Where(b => command.Bets.Contains(b.Id))
            .Where(b => !b.Race.IsCompleted)
            .ToListAsync(cancellationToken);

        if(bets.Count != command.Bets.Count)
        {
            return Result.Failure(TicketErrors.NotFound());
        }

        // now check if there is only one bet per race
        var distinctRaceIds = bets.Select(b => b.Race.Id).Distinct().ToList();
        if (bets.Count != distinctRaceIds.Count)
        {
            return Result.Failure(TicketErrors.OneBetPerRaceAllowed());
        }

        int totalOdds = 1;

        var ticket = new Ticket { 
            Id = command.Id,
            Bets = [.. bets.Select(b => new TicketBet
            {
                Bet = b,
                Odds = b.Odds,
                Status = BetStatus.InProgress,
            })],
            Payin = command.Payin,
            TotalOdds = totalOdds,
            WinAmount = 10,
            CreatedAt = dateTimeProvider.UtcNow,
        };

        context.Tickets.Add(ticket);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
