using System.Text.Json;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Messaging;

internal sealed class RedisSubscriberService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisSubscriberService> _logger;

    public RedisSubscriberService(
        IConnectionMultiplexer redis,
        ILogger<RedisSubscriberService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ISubscriber subscriber = _redis.GetSubscriber();
        
        await subscriber.SubscribeAsync(RedisChannel.Literal(SharedKernel.RedisChannels.RaceUpdates),  (channel, value) =>
        {
            try
            {
                _logger.LogInformation("Received race update message: {Message}", value);
                
                // Parse the message
                RaceCreatedMessage? message = JsonSerializer.Deserialize<RaceCreatedMessage>(value.ToString());
                if (message is not null)
                {
                    //await HandleRaceCreatedAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing race update message: {Message}", value);
            }
        });

        _logger.LogInformation("Redis subscriber service started and listening for race updates");

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

   
}

public sealed record RaceCreatedMessage(Guid RaceId); 
