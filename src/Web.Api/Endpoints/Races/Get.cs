using Application.Abstractions.Messaging;
using Application.Races.Get;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Races;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("races", async (
            IQueryHandler<GetRacesQuery, List<RaceResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRacesQuery();

            Result<List<RaceResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Races);
    }
}
