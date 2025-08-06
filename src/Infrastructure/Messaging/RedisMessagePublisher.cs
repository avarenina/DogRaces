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
        string json = JsonSerializer.Serialize(message);
        await PublishAsync(channel, json);
    }

    public async Task PublishAsync(string channel, string message)
    {
        await _redis.GetSubscriber().PublishAsync(RedisChannel.Literal(channel), message);
    }
}
