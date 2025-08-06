using Domain.Bets;
using SharedKernel;

namespace Domain.Races;

public class Race : Entity
{
    protected Race() { }

    public Race(
    Guid id,
    double[] probabilities,
    DateTime startTime,
    DateTime createdAt,
    RaceStatus status)
    {
        Id = id;
        Probabilities = probabilities;
        StartTime = startTime;
        CreatedAt = createdAt;
        Status = status;
        Bets = [];
    }

    public Guid Id { get; set; }
    public string? Result { get; set; }
    public virtual ICollection<Bet> Bets { get; set; } = [];
    public bool IsCompleted { get; set; }
    public double[] Probabilities { get; set; } = [];
    public DateTime StartTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RaceStatus Status { get; set; }
}
