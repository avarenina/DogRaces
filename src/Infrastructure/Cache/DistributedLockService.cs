using Application.Abstractions;
using Medallion.Threading;

namespace Infrastructure.Cache;

public class DistributedLockService : IDistributedLockService
{
    private readonly IDistributedLockProvider _lockProvider;

    public DistributedLockService(IDistributedLockProvider lockProvider)
    {
        _lockProvider = lockProvider;
    }

    public async Task<IDisposable?> AcquireAsync(string resource, TimeSpan timeout, CancellationToken cancellationToken)
    {
        // Wait up to timeout, returns null if lock not acquired
        IDistributedSynchronizationHandle handle = await _lockProvider.AcquireLockAsync(resource, timeout, cancellationToken);
        return handle;
    }
}
