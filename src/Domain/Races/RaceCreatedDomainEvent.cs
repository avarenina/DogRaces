using SharedKernel;

namespace Domain.Races;

public sealed record RaceCreatedDomainEvent(Guid RaceId) : IDomainEvent;
