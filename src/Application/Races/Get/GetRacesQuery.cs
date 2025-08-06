using Application.Abstractions.Messaging;

namespace Application.Races.Get;

public sealed record GetRacesQuery(bool IgnoreCache = false) : IQuery<List<RaceResponse>>;
