using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Races.Create;

internal sealed class CreateRaceCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateRaceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRaceCommand command, CancellationToken cancellationToken)
    {
       
        var race = new Race
        {
            StartTime = command.StartTime,
            CreatedAt = dateTimeProvider.UtcNow,
        };

        race.Raise(new RaceCreatedDomainEvent(race.Id));

        context.Races.Add(race);

        await context.SaveChangesAsync(cancellationToken);

        return race.Id;
    }
}
