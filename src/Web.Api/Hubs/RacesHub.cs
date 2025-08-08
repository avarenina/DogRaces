using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Races.Get;
using Microsoft.AspNetCore.SignalR;
using SharedKernel;

namespace Web.Api.Hubs;

public sealed class RacesHub : Hub
{
    private readonly IClientsNotifier _clientsNotifier;
    public RacesHub(IClientsNotifier clientsNotifier)
    {
        _clientsNotifier = clientsNotifier;
    }

    public async Task JoinRacesGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "RacesGroup");
        await _clientsNotifier.NotifyNewUpcomingRacesAsync();
        await _clientsNotifier.NotifyBalanceChangedAsync();
    }

    public async Task LeaveRacesGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "RacesGroup");
    }
}
