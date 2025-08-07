using SharedKernel;

namespace Domain.Ticket;
public static class TicketErrors
{
    public static Error NotFound() => Error.NotFound(
        "Bets.NotFound",
        $"Some of the bets provided on the ticket are not found");

    public static Error OneBetPerRaceAllowed() => Error.Problem(
       "Bets.OneBetPerRaceAllowed",
       $"Some of the bets provided on the ticket are not found");
}
