using Domain.Bets;
using Domain.Races;
using Domain.Ticket;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Race> Races { get; }
    DbSet<Bet> Bets { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketBet> TicketBets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
