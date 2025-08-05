using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Races.Delete;

internal sealed class DeleteRaceCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteRaceCommand>
{
    public async Task<Result> Handle(DeleteRaceCommand command, CancellationToken cancellationToken)
    {
        Race? race = await context.Races
            .SingleOrDefaultAsync(t => t.Id == command.RaceId, cancellationToken);

        if (race is null)
        {
            return Result.Failure(RaceErrors.NotFound(command.RaceId));
        }

        context.Races.Remove(race);

        race.Raise(new RaceDeletedDomainEvent(race.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
