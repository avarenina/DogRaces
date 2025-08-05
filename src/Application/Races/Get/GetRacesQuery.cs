using Application.Abstractions.Messaging;

namespace Application.Races.Get;

public sealed record GetRacesQuery() : IQuery<List<RaceResponse>>;
