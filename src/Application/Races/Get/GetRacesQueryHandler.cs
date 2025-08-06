using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Bets;
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
                List<RaceResponse>? racesCache = JsonSerializer.Deserialize<List<RaceResponse>>(cachedData, JsonConfig.DefaultOptions);
                if (racesCache != null)
                {

                    return racesCache;
                }
            }
        }

        List<RaceResponse> races = await context.Races
        .Where(r => r.StartTime > dateTimeProvider.UtcNow)
        .Include(r => r.Bets)
        .OrderByDescending(r => r.StartTime)
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

    public static class JsonConfig
    {
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            Converters = { new BetJsonConverter() }
        };
    }
    public class BetJsonConverter : JsonConverter<Bet>
    {
        

        public override Bet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("Type", out JsonElement betTypeProp))
            {
                throw new JsonException("Missing 'betType' property");
            }

           
            if (!betTypeProp.TryGetInt32(out int betTypeValue))
            {
                throw new JsonException("'betType' is not a valid integer");
            }

            if (!Enum.IsDefined(typeof(BetType), betTypeValue))
            {
                throw new JsonException($"Invalid BetType value: {betTypeValue}");
            }

            var betType = (BetType)betTypeValue;

            Bet result = betType switch
            {
                BetType.Winner => JsonSerializer.Deserialize<WinnerBet>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown BetType: {betType}")
            };

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Bet value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
