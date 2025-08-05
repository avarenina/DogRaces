using Application.Abstractions.Messaging;
using Application.Races.Finish;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Races;

internal sealed class Complete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("races/{id:guid}/complete", async (
            Guid id,
            ICommandHandler<FinishRaceCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new FinishRaceCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Races);
    }
}
