using Application.Abstractions.Messaging;

namespace Application.Tickets.ProcessTicketBets;

public sealed record ProcessTicketBetsCommand(int BatchSize) : ICommand;


