using SharedKernel;

namespace Domain.Races;

public static class RaceErrors
{
    public static Error AlreadyCompleted(Guid raceId) => Error.Problem(
        "Races.AlreadyCompleted",
        $"The race with Id = '{raceId}' is already completed.");

    public static Error NotFound(Guid raceId) => Error.NotFound(
        "Races.NotFound",
        $"The race with the Id = '{raceId}' was not found");
}
