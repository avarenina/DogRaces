using SharedKernel;

namespace Domain.Tickets;
public static class TicketErrors
{
    public static Error NotFound() => Error.NotFound(
        "Bets.NotFound",
        $"Some of the bets provided on the ticket are not found or race alerady started");

    public static Error OneBetPerRaceAllowed() => Error.Problem(
       "Bets.OneBetPerRaceAllowed",
       $"Only one bet per race is allowed on a ticket");


    public static Error AlreadyExists(Guid ticketId) => Error.Conflict(
        "Tickets.AlreadyExists",
        $"A ticket with Id = '{ticketId}' already exists.");

    public static Error TotalOddsOutOfRange(decimal minTotalOdds, decimal maxTotalOdds) => Error.Problem(
        "Tickets.TotalOddsOutOfRange",
        $"Total odds must be between {minTotalOdds} and {maxTotalOdds}.");

    public static Error MaxWinExceeded(decimal maxWin) => Error.Problem(
        "Tickets.MaxWinExceeded",
        $"Max win exceeded. Maximum allowed win is {maxWin}.");
}
