using Domain.Bets;

namespace Application.Tickets.Purchase;

public interface ITicketPurchaseValidator
{
    bool Validate(
        PurchaseTicketCommand command,
        IReadOnlyCollection<Bet> bets,
        TicketValidationOptions options,
        out SharedKernel.Error? error);
}


