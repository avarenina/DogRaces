using SharedKernel;

namespace Domain.Races;

public sealed record RaceFinishedDomainEvent(Guid RaceId) : IDomainEvent;
