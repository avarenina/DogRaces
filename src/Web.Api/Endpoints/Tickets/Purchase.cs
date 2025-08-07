using Application.Abstractions.Messaging;
using Application.Races.Finish;
using Application.Tickets.Purchase;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tickets;

internal sealed class Purchase: IEndpoint
{
    public sealed class Request
    {
        public Guid TicketId { get; set; }
        public decimal Payin { get; set; }
        public List<Guid> Bets { get; set; }
    }
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tickets/purchase", async (
            Request request,
            ICommandHandler<PurchaseTicketCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new PurchaseTicketCommand()
            {
                Id = request.TicketId,    
                Payin = request.Payin,
                Bets = request.Bets,
            };

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Tickets);
    }
}
