using FluentValidation;

namespace Application.Races.Create;

public class CreateRaceCommandValidator : AbstractValidator<CreateRaceCommand>
{
    public CreateRaceCommandValidator()
    {
        RuleFor(c => c.AmountOfRacesToCreate).GreaterThan(1);
        RuleFor(c => c.TimeBetweenRaces).GreaterThan(1);
    }
}
