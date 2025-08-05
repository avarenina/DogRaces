using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Races.Get;

internal sealed class GetRacesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetRacesQuery, List<RaceResponse>>
{
    public async Task<Result<List<RaceResponse>>> Handle(GetRacesQuery query, CancellationToken cancellationToken)
    {
        List<RaceResponse> races = await context.Races
            .Select(r => new RaceResponse
            {
                Id = r.Id,
                IsCompleted = r.IsCompleted,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return races;
    }
}
