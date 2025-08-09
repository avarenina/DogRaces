using Domain.Bets;
using Domain.Races;
using Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Race> Races { get; }
    DbSet<Bet> Bets { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketBet> TicketBets { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
