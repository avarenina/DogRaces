using SharedKernel;

namespace Domain.Races;

public sealed record RaceDeletedDomainEvent(Guid RaceId) : IDomainEvent;
