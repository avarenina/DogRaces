using Domain.Bets;
using Domain.Races;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Race> Races { get; }
    DbSet<Bet> Bets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
