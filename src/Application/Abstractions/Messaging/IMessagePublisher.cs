namespace Application.Abstractions.Messaging;
public interface IMessagePublisher
{
    Task PublishAsync<T>(string channel, T message);
}
