using Application.Abstractions.Messaging;
using Application.Races.Create;
using Domain.Races;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Races;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public DateTime StartTime { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("races", async (
            Request request,
            ICommandHandler<CreateRaceCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateRaceCommand
            {
               StartTime = request.StartTime,
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Races);
    }
}
