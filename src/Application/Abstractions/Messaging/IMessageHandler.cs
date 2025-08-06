namespace Application.Abstractions.Messaging;
public interface IMessageHandler<in T>
{
    Task HandleAsync(T message);
}
