using Application.Abstractions.Messaging;
using Application.Races.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Races;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("races/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteRaceCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteRaceCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Races);
    }
}
