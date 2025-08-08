using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Abstractions;
using Application.Races.Finish;
using Microsoft.Extensions.Logging;
using Application.Tickets.ProcessTickets;

namespace Infrastructure.Messaging.Handlers;

public sealed class TicketWinMessageHandler : IMessageHandler<TicketWinMessage>
{
    private readonly IClientsNotifier _racesNotifier;
    private readonly IWalletService _walletService;
    private readonly ILogger<TicketWinMessageHandler> _logger;

    public TicketWinMessageHandler(IClientsNotifier racesNotifier, ILogger<TicketWinMessageHandler> logger, IWalletService walletService)
    {
        _racesNotifier = racesNotifier;
        _logger = logger;
        _walletService = walletService;
    }

    public async Task HandleAsync(TicketWinMessage message)
    {
        try
        {
            await _walletService.FundAsync(Guid.NewGuid(), message.WinAmount, message.TicketId, CancellationToken.None);
            await _racesNotifier.NotifyBalanceChangedAsync();
            _logger.LogInformation("Successfully triggered balance update for winning ticket {TicketId}", message.TicketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling balance update message for ticket {TicketId}", message.TicketId);
        }
    }
}
