using Application.Abstractions.Messaging;
using Domain.Races;

namespace Application.Races.Create;

public sealed class CreateRaceCommand : ICommand<Guid>
{
    public DateTime StartTime { get; set; }
}
