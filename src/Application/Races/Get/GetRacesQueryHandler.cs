using System.Text.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Races.Get;

internal sealed class GetRacesQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider, IDistributedCache distributedCache)
    : IQueryHandler<GetRacesQuery, List<RaceResponse>>
{
    public async Task<Result<List<RaceResponse>>> Handle(GetRacesQuery query, CancellationToken cancellationToken)
    {
        if(!query.IgnoreCache)
        {
            // Try to read from cache
            string? cachedData = await distributedCache.GetStringAsync(CacheKeys.UpcomingRaces, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                List<RaceResponse>? racesCache = JsonSerializer.Deserialize<List<RaceResponse>>(cachedData);
                if (racesCache != null)
                {

                    return racesCache;
                }
            }
        }

        List<RaceResponse> races = await context.Races
        .Where(r => r.StartTime > dateTimeProvider.UtcNow)
        .OrderByDescending(r => r.StartTime)
        .Select(r => new RaceResponse
        {
            Id = r.Id,
            StartTime = r.StartTime,
            IsCompleted = r.IsCompleted,
            CreatedAt = r.CreatedAt,
            CompletedAt = r.CompletedAt
        })
        .ToListAsync(cancellationToken);

        // if there arent any races dont set the cache
        if(races.Count > 0)
        {
            TimeSpan cacheLiveTime = races[0].StartTime - dateTimeProvider.UtcNow;

            // Store in cache
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheLiveTime
            };

            string serialized = JsonSerializer.Serialize(races);

            await distributedCache.SetStringAsync(CacheKeys.UpcomingRaces, serialized, cacheEntryOptions, cancellationToken);
        }

        return races;
    }
}
