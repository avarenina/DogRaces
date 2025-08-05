using Application.Abstractions.Messaging;
using Application.Races.Get;
using Microsoft.AspNetCore.SignalR;
using SharedKernel;

namespace Web.Api.Hubs;

public sealed class RacesHub : Hub
{
    private readonly IQueryHandler<GetRacesQuery, List<RaceResponse>> _getRacesQueryHandler;

    public RacesHub(IQueryHandler<GetRacesQuery, List<RaceResponse>> getRacesQueryHandler)
    {
        _getRacesQueryHandler = getRacesQueryHandler;
    }

    public async Task JoinRacesGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "RacesGroup");
        await SendUpcomingRaces();
    }

    public async Task LeaveRacesGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "RacesGroup");
    }

    public async Task SendUpcomingRaces()
    {
        var query = new GetRacesQuery();
        Result<List<RaceResponse>> result = await _getRacesQueryHandler.Handle(query, CancellationToken.None);
        
        if (result.IsSuccess)
        {
            await Clients.Group("RacesGroup").SendAsync("ReceiveUpcomingRaces", result.Value);
        }
    }
} 
