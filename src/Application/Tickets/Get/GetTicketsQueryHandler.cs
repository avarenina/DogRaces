using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tickets.Get;

internal sealed class GetTicketsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTicketsQuery, PaginatedResponse<TicketResponse>>
{
    public async Task<Result<PaginatedResponse<TicketResponse>>> Handle(GetTicketsQuery query, CancellationToken cancellationToken)
    {
        int page = query.Page < 1 ? 1 : query.Page;
        int pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        IQueryable<Domain.Ticket.Ticket> baseQuery = context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Bets)
            .ThenInclude(tb => tb.Bet);

        int totalCount = await baseQuery.CountAsync(cancellationToken);

        List<TicketResponse> items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketResponse
            {
                Id = t.Id,
                Status = t.Status,
                Payin = t.Payin,
                TotalOdds = t.TotalOdds,
                WinAmount = t.WinAmount,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                Bets = t.Bets.Select(tb => new TicketBetResponse
                {
                    BetId = tb.Bet.Id,
                    Odds = tb.Odds,
                    Status = tb.Status,
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        var response = new PaginatedResponse<TicketResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };

        return response;
    }
}


