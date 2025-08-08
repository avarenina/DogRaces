using Application.Abstractions.Messaging;

namespace Application.Tickets.Process;

public sealed record ProcessTicketsCommand(int BatchSize = 100) : ICommand;


