using FluentValidation;

namespace Application.Races.Create;

public class CreateRaceCommandValidator : AbstractValidator<CreateRaceCommand>
{
    public CreateRaceCommandValidator()
    {
        RuleFor(c => c.StartTime).GreaterThan(DateTime.Now);
    }
}
