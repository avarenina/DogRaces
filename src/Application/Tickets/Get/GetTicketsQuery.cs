using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Tickets.Get;

public sealed record GetTicketsQuery(int Page = 1, int PageSize = 20) : IQuery<PaginatedResponse<TicketResponse>>;


