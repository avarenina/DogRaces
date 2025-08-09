namespace Domain.Tickets;

public sealed class TicketValidationOptions
{
    public decimal MinPayin { get; init; } = 1m;
    public decimal MaxPayin { get; init; } = 100m;
    public int MaxBets { get; init; } = 10;
    public decimal MinTotalOdds { get; init; } = 1.01m; // product of odds should be at least 1
    public decimal MaxTotalOdds { get; init; } = 1000m;
    public decimal MaxWin { get; init; } = 10000m;
}


