using SharedKernel;

namespace Domain.Races;

public sealed class Race : Entity
{
    public Guid Id { get; set; }
    public string? Result { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public RaceStatus Status { get; set; }
}
