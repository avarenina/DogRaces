using Application.Abstractions.Messaging;

namespace Application.Races.GetById;

public sealed record GetRaceByIdQuery(Guid RaceId) : IQuery<RaceResponse>;
