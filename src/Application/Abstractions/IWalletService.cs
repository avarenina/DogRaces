using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Application.Abstractions;
public interface IWalletService
{
    Task<Result> ReserveFundsAsync(Guid userId, decimal amount, Guid transactionId, CancellationToken cancellationToken);
    Task<Result> ConfirmFundsAsync(Guid transactionId, CancellationToken cancellationToken);
    Task<Result> RollbackFundsAsync(Guid transactionId, CancellationToken cancellationToken);
}
