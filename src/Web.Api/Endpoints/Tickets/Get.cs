using Application.Abstractions.Messaging;
using Application.Tickets.Get;
using SharedKernel;
using Web.Api.Infrastructure;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Tickets;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tickets", async (
            int? page,
            int? pageSize,
            IQueryHandler<GetTicketsQuery, PaginatedResponse<TicketResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTicketsQuery(page ?? 1, pageSize ?? 20);

            Result<PaginatedResponse<TicketResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tickets);
    }
}


