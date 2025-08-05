using Application.Abstractions.Messaging;

namespace Application.Races.Finish;

public sealed record FinishRaceCommand(Guid RaceId) : ICommand
{
    public string Result { get; set; }
}
