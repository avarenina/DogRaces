using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Races.GetById;

internal sealed class GetRaceByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetRaceByIdQuery, RaceResponse>
{
    public async Task<Result<RaceResponse>> Handle(GetRaceByIdQuery query, CancellationToken cancellationToken)
    {
        RaceResponse? race = await context.Races
            .Where(r => r.Id == query.RaceId)
            .Select(r => new RaceResponse
            {
                Id = r.Id,
                IsCompleted = r.IsCompleted,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (race is null)
        {
            return Result.Failure<RaceResponse>(RaceErrors.NotFound(query.RaceId));
        }

        return race;
    }
}
