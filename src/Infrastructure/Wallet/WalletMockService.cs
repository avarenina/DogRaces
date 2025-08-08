using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions;
using SharedKernel;

namespace Infrastructure.Wallet;
public class WalletMockService : IWalletService
{
    private decimal _balance = 100m; 
    private readonly Dictionary<Guid, decimal> _reservations = [];
    private readonly Lock _lock = new(); 

    public Task<Result> ReserveFundsAsync(Guid userId, decimal amount, Guid transactionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_balance < amount)
            {
                return Task.FromResult(Result.Failure(Error.Problem("Wallet.InsufficientFunds", "Not enough funds in wallet.")));
            }

            _balance -= amount;
            _reservations[transactionId] = amount;

            return Task.FromResult(Result.Success());
        }
    }

    public Task<Result> ConfirmFundsAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if(_reservations.Remove(transactionId))
            {
                return Task.FromResult(Result.Success());
            }
               
            return Task.FromResult(Result.Failure(Error.Problem("Wallet.ConfirmError", "No reservation found to confirm.")));
        }
    }

    public Task<Result> RollbackFundsAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_reservations.TryGetValue(transactionId, out decimal reservedAmount))
            {
                _balance += reservedAmount;
                _reservations.Remove(transactionId);
            }

            return Task.FromResult(Result.Success());
        }
    }

    public Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken) 
    {
        lock (_lock)
        {
            return Task.FromResult(_balance);
        }
    }

    public Task<Result> FundAsync(Guid userId, decimal amount, Guid transactionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _balance += amount;
            _reservations[transactionId] = amount;

            return Task.FromResult(Result.Success());
        }
    }
}
