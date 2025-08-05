using FluentValidation;

namespace Application.Races.Finish;

internal sealed class FinishRaceCommandValidator : AbstractValidator<FinishRaceCommand>
{
    public FinishRaceCommandValidator()
    {
        RuleFor(c => c.RaceId).NotEmpty();
    }
}
