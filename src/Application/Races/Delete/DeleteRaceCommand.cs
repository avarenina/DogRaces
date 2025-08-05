using Application.Abstractions.Messaging;

namespace Application.Races.Delete;

public sealed record DeleteRaceCommand(Guid RaceId) : ICommand;
