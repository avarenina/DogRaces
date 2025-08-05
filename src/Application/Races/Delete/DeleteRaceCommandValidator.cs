using FluentValidation;

namespace Application.Races.Delete;

internal sealed class DeleteRaceCommandValidator : AbstractValidator<DeleteRaceCommand>
{
    public DeleteRaceCommandValidator()
    {
        RuleFor(c => c.RaceId).NotEmpty();
    }
}
