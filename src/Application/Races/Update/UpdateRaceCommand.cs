using Application.Abstractions.Messaging;

namespace Application.Races.Update;

public sealed record UpdateRaceCommand(
    Guid RaceId,
    string Result) : ICommand;
