using System.Text.Json;
using Application.Abstractions.Messaging;
using StackExchange.Redis;

namespace Infrastructure.Messaging;
public class RedisMessagePublisher : IMessagePublisher
{
    private readonly IConnectionMultiplexer _redis;

    public RedisMessagePublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        string type = typeof(T).Name; // e.g., "RaceCreatedMessage"
        string payload = JsonSerializer.Serialize(message);
        var envelope = new RedisEnvelope(type, payload);
        string envelopeJson = JsonSerializer.Serialize(envelope);
        await PublishAsync(channel, envelopeJson);
    }
    
    public async Task PublishAsync(string channel, string message)
    {
        await _redis.GetSubscriber().PublishAsync(RedisChannel.Literal(channel), message);
    }
}

public sealed record RedisEnvelope(string Type, string Payload);
