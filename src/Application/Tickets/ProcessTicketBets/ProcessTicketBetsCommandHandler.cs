using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Bets;
using Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using SharedKernel;

namespace Application.Tickets.ProcessTicketBets;

internal sealed class ProcessTicketBetsCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<ProcessTicketBetsCommand>
{
    public async Task<Result> Handle(ProcessTicketBetsCommand command, CancellationToken cancellationToken)
    {
        while (true)
        {
            List<TicketBet> ticketBetsToProcess = await context.TicketBets
                .Where(tb => tb.Status == BetStatus.InProgress && tb.Bet.Status != BetStatus.InProgress)
                .Include(tb => tb.Bet)
                .Take(command.BatchSize)
                .ToListAsync(cancellationToken);

            if (ticketBetsToProcess.Count == 0)
            {
                break;
            }

            foreach (TicketBet ticketBet in ticketBetsToProcess)
            {
                ticketBet.ResolveStatus(); 
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
