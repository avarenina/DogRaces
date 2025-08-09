using Domain.Bets;
using Domain.Tickets;

namespace Application.Tickets.Get;

public sealed class TicketResponse
{
    public Guid Id { get; set; }
    public TicketStatus Status { get; set; }
    public decimal Payin { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal WinAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<TicketBetResponse> Bets { get; set; } = Array.Empty<TicketBetResponse>();
}

public sealed class TicketBetResponse
{
    public Guid BetId { get; set; }
    public decimal Odds { get; set; }
    public BetStatus Status { get; set; }
}


