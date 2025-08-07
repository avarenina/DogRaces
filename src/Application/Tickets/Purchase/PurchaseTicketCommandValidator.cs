using FluentValidation;

namespace Application.Tickets.Purchase;

internal sealed class PurchaseTicketCommandValidator : AbstractValidator<PurchaseTicketCommand>
{
    public PurchaseTicketCommandValidator(TicketValidationOptions options)
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id must be a valid non-empty GUID.");

        RuleFor(c => c.Payin)
            .InclusiveBetween(options.MinPayin, options.MaxPayin)
            .WithMessage($"Payin must be between {options.MinPayin} and {options.MaxPayin}.");

        RuleFor(c => c.Bets)
            .NotNull().WithMessage("Bets list must not be null.")
            .Must(bets => bets.Any()).WithMessage("Bets list must not be empty.")
            .Must(bets => bets.Count <= options.MaxBets).WithMessage($"You can select up to {options.MaxBets} bets maximum.")
            .Must(bets => bets.Distinct().Count() == bets.Count).WithMessage("Duplicate bet IDs are not allowed.");

        RuleForEach(c => c.Bets)
            .NotEqual(Guid.Empty).WithMessage("Each Bet ID must be a valid non-empty GUID.");
    }
}
