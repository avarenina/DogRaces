using Domain.Bets;
using SharedKernel;

namespace Domain.Ticket;
public class Ticket : Entity
{
    public Guid Id { get; set; }
    public TicketStatus Status { get; set; }
    public virtual ICollection<TicketBet> Bets { get; set; } = [];
    public decimal Payin {  get; set; }
    public decimal TotalOdds { get; set; }
    public decimal WinAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
