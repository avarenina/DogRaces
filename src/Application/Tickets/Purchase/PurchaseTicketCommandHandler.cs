using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions;
using Domain.Bets;
using Domain.Ticket;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tickets.Purchase;
internal sealed class PurchaseTicketCommandHandler(
    IApplicationDbContext context,
    ITicketFactory ticketFactory,
    ITicketPurchaseValidator ticketPurchaseValidator,
    TicketValidationOptions ticketValidationOptions) 
    : ICommandHandler<PurchaseTicketCommand>
{
    public async Task<Result> Handle(PurchaseTicketCommand command, CancellationToken cancellationToken)
    {
        
        List<Bet> bets = await context.Bets
            .Include(b => b.Race)
            .Where(b => command.Bets.Contains(b.Id))
            .Where(b => !b.Race.IsCompleted)
            .ToListAsync(cancellationToken);

        if (bets.Count != command.Bets.Count)
        {
            return Result.Failure(TicketErrors.NotFound());
        }

        // Business rules without DB access inside validators
        if (!ticketPurchaseValidator.Validate(command, bets, ticketValidationOptions, out Error? validationError))
        {
            return Result.Failure(validationError ?? Error.Problem("Tickets.ValidationFailed", "Ticket validation failed."));
        }

        // Create ticket as Pending using factory
        Ticket ticket = ticketFactory.Create(command.Id, bets, command.Payin);

        context.Tickets.Add(ticket);

        await context.SaveChangesAsync(cancellationToken);

        // Update status to Success after persistence
        ticket.Status = TicketStatus.Success;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
