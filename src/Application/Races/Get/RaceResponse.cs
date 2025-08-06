using Domain.Bets;
using Domain.Races;

namespace Application.Races.Get;

public class RaceResponse
{
    public Guid Id { get; set; }
    public string? Result { get; set; }
    public bool IsCompleted { get; set; }
    public virtual ICollection<BetResponse> Bets { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RaceStatus Status { get; set; }
}
