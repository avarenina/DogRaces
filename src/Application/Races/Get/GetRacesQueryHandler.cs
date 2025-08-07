using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Bets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Races.Get;

internal sealed class GetRacesQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider, IDistributedCache distributedCache, IDistributedLockService lockService)
    : IQueryHandler<GetRacesQuery, List<RaceResponse>>
{
    public async Task<Result<List<RaceResponse>>> Handle(GetRacesQuery query, CancellationToken cancellationToken)
    {
        if (!query.IgnoreCache)
        {
            // Try to read from cache
            string? cachedData = await distributedCache.GetStringAsync(CacheKeys.UpcomingRaces, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                List<RaceResponse>? racesCache = JsonSerializer.Deserialize<List<RaceResponse>>(cachedData, JsonConfig.DefaultOptions);
                if (racesCache != null)
                {
                    return racesCache;
                }
            }
        }

        // Try to acquire distributed lock with 2 second timeout
        IDisposable? lockHandle = await lockService.AcquireAsync("locks:races:upcoming", TimeSpan.FromSeconds(10), cancellationToken);

        if (lockHandle == null)
        {
            // Could not get the lock in 10 second: fallback to stale cache or error
            string? staleData = await distributedCache.GetStringAsync(CacheKeys.UpcomingRaces, cancellationToken);
            if (!string.IsNullOrEmpty(staleData))
            {
                List<RaceResponse>? staleRaces = JsonSerializer.Deserialize<List<RaceResponse>>(staleData, JsonConfig.DefaultOptions);
                if (staleRaces != null)
                {
                    return staleRaces;
                }
            }
            throw new TimeoutException("Could not acquire distributed lock to refresh upcoming races cache.");
        }

        using (lockHandle)
        {

            List<RaceResponse> races = await context.Races
            .Where(r => r.StartTime > dateTimeProvider.UtcNow)
            .Where(r => !r.IsCompleted)
            .Include(r => r.Bets)
            .OrderBy(r => r.StartTime)
            .AsNoTracking()
            .Select(r => new RaceResponse
            {
                Id = r.Id,
                StartTime = r.StartTime,
                Bets = r.Bets.Select(b => new BetResponse
                {
                    Id = b.Id,
                    Odds = b.Odds,
                    Runners = b.Runners,
                    Status = b.Status,
                    Type = b.Type,
                }).ToList(),
                IsCompleted = r.IsCompleted,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            })
            .ToListAsync(cancellationToken);

            // if there arent any races dont set the cache
            if (races.Count > 0)
            {
                TimeSpan cacheLiveTime = races[0].StartTime - dateTimeProvider.UtcNow;

                // Store in cache
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheLiveTime
                };

                string serialized = JsonSerializer.Serialize(races, JsonConfig.DefaultOptions);

                await distributedCache.SetStringAsync(CacheKeys.UpcomingRaces, serialized, cacheEntryOptions, cancellationToken);
            }

            return races;
        }
    }
}
