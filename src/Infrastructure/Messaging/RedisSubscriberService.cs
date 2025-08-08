using System.Text.Json;
using Application.Abstractions.Messaging;
using Application.Races.Finish;
using Application.Races.Create;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using Application.Tickets.ProcessTickets;

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
                RedisEnvelope? envelope = JsonSerializer.Deserialize<RedisEnvelope>(value.ToString());
                if (envelope is null)
                {
                    return;
                }

                switch (envelope.Type)
                {
                    case "RaceCreatedMessage":
                        RaceCreatedMessage? created = JsonSerializer.Deserialize<RaceCreatedMessage>(envelope.Payload);
                        if (created is not null)
                        {
                            using IServiceScope scope = _serviceProvider.CreateScope();
                            IMessageHandler<RaceCreatedMessage> handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<RaceCreatedMessage>>();
                            await handler.HandleAsync(created);
                        }
                        break;
                    case "RaceFinishedMessage":
                        RaceFinishedMessage? finished = JsonSerializer.Deserialize<RaceFinishedMessage>(envelope.Payload);
                        if (finished is not null)
                        {
                            using IServiceScope scope = _serviceProvider.CreateScope();
                            IMessageHandler<RaceFinishedMessage> handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<RaceFinishedMessage>>();
                            await handler.HandleAsync(finished);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing race update message: {Message}", value);
            }
        });

        await subscriber.SubscribeAsync(RedisChannel.Literal(SharedKernel.RedisChannels.TicketUpdates), async (channel, value) =>
        {
            try
            {
                RedisEnvelope? envelope = JsonSerializer.Deserialize<RedisEnvelope>(value.ToString());
                if (envelope is null)
                {
                    return;
                }

                switch (envelope.Type)
                {
                    case "TicketWinMessage":
                        TicketWinMessage? win = JsonSerializer.Deserialize<TicketWinMessage>(envelope.Payload);
                        if (win is not null)
                        {
                            using IServiceScope scope = _serviceProvider.CreateScope();
                            IMessageHandler<TicketWinMessage> handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TicketWinMessage>>();
                            await handler.HandleAsync(win);
                        }
                        break;
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

