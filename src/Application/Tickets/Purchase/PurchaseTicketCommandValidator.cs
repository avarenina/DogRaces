using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Application.Tickets.Purchase;
public class PurchaseTicketCommandValidator : AbstractValidator<PurchaseTicketCommand>
{
    public PurchaseTicketCommandValidator()
    {
        RuleFor(c => c.Payin)
            .InclusiveBetween(1, 100)
            .WithMessage("Payin must be between 1 and 100.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id must be a valid non-empty GUID.");

        RuleFor(c => c.Bets)
            .NotNull().WithMessage("Bets list must not be null.")
            .Must(bets => bets.Any()).WithMessage("Bets list must not be empty.")
            .Must(bets => bets.Count <= 10).WithMessage("You can select up to 10 bets maximum.");

        RuleForEach(c => c.Bets)
            .NotEqual(Guid.Empty).WithMessage("Each Bet ID must be a valid non-empty GUID.");
    }
}
