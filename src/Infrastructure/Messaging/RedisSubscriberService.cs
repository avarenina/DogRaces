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
    private readonly IServiceProvider _serviceProvider;

    public RedisSubscriberService(
        IConnectionMultiplexer redis,
        ILogger<RedisSubscriberService> logger,
        IServiceProvider serviceProvider)
    {
        _redis = redis;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ISubscriber subscriber = _redis.GetSubscriber();
        
        await subscriber.SubscribeAsync(RedisChannel.Literal(SharedKernel.RedisChannels.RaceUpdates), async (channel, value) =>
        {
            try
            {
                _logger.LogInformation("Received race update message: {Message}", value);
                
                // Parse the message
                RaceCreatedMessage? message = JsonSerializer.Deserialize<RaceCreatedMessage>(value.ToString());
                if (message is not null)
                {
                    await HandleRaceCreatedAsync(message);
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

    private async Task HandleRaceCreatedAsync(RaceCreatedMessage message)
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            
            // Get the message handler and process the message
            IMessageHandler<RaceCreatedMessage> messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<RaceCreatedMessage>>();
            await messageHandler.HandleAsync(message);
            
            _logger.LogInformation("Processed race created event for race {RaceId}", message.RaceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling race created event for race {RaceId}", message.RaceId);
        }
    }
}

public sealed record RaceCreatedMessage(Guid RaceId); 