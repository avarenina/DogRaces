using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Tickets.Process;
using Domain.Bets;
using Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tickets.ProcessTickets;

internal sealed class ProcessTicketsCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ProcessTicketsCommand>
{
    public async Task<Result> Handle(ProcessTicketsCommand command, CancellationToken cancellationToken)
    {
        while (true)
        {
            List<Ticket> tickets = await context.Tickets
                .Where(t => t.Status == TicketStatus.Success && t.CompletedAt == null)
                .Where(t => !t.Bets.Any(b => b.Status == BetStatus.InProgress))
                .Include(t => t.Bets)
                .OrderBy(t => t.CreatedAt)
                .Take(command.BatchSize)
                .ToListAsync(cancellationToken);

            if (tickets.Count == 0)
            {
                break;
            }

            foreach (Ticket ticket in tickets)
            {
                ticket.Complete(dateTimeProvider.UtcNow);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}

