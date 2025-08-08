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
    TicketValidationOptions ticketValidationOptions,
    IWalletService walletService)
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

        if (!ticketPurchaseValidator.Validate(command, bets, ticketValidationOptions, out Error? validationError))
        {
            return Result.Failure(validationError ?? Error.Problem("Tickets.ValidationFailed", "Ticket validation failed."));
        }

        // Reserve funds from wallet
        Result reservationResult = await walletService.ReserveFundsAsync(Guid.NewGuid(), command.Payin, command.Id, cancellationToken);
        if (reservationResult.IsFailure)
        {
            return reservationResult;
        }

        try
        {
            // Create ticket as Pending
            Ticket ticket = ticketFactory.Create(command.Id, bets, command.Payin);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync(cancellationToken);

            // Confirm wallet reservation
            Result confirmResult = await walletService.ConfirmFundsAsync(command.Id, cancellationToken);
            if (confirmResult.IsFailure)
            {
                await walletService.RollbackFundsAsync(command.Id, cancellationToken);

                ticket.Status = TicketStatus.Rejected;
                await context.SaveChangesAsync(cancellationToken);

                return Result.Failure(Error.Problem("Wallet.ConfirmFailed", "Failed to confirm wallet reservation."));
            }

            // Notify clients for balance change
            ticket.Raise(new TicketPurchaseDomainEvent(ticket.Id, ticket.Payin));

            // Mark ticket as success
            ticket.Status = TicketStatus.Success;
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Roll back wallet reservation if any error occurs
            await walletService.RollbackFundsAsync(command.Id, cancellationToken);

            // Optionally log the exception here
            return Result.Failure(Error.Problem("Ticket.CreationFailed", $"Ticket creation failed: {ex.Message}"));
        }
    }
}
