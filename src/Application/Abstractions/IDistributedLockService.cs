namespace Application.Abstractions;

public interface IDistributedLockService
{
    Task<IDisposable?> AcquireAsync(string resource, TimeSpan timeout, CancellationToken cancellationToken);
}
