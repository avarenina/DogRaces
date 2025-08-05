using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Races.Update;

internal sealed class UpdateRaceCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<UpdateRaceCommand>
{
    public async Task<Result> Handle(UpdateRaceCommand command, CancellationToken cancellationToken)
    {
        Race? race = await context.Races
            .SingleOrDefaultAsync(t => t.Id == command.RaceId, cancellationToken);

        if (race is null)
        {
            return Result.Failure(RaceErrors.NotFound(command.RaceId));
        }

        race.Result = command.Result;

        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
