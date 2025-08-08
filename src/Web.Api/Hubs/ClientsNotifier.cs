using Application.Abstractions;
using Application.Abstractions.Messaging;
using Application.Races.Finish;
using Application.Races.Get;
using Microsoft.AspNetCore.SignalR;
using SharedKernel;

namespace Web.Api.Hubs;

internal sealed class ClientsNotifier : IClientsNotifier
{
    private readonly IHubContext<RacesHub> _hubContext;
    private readonly IQueryHandler<GetRacesQuery, List<RaceResponse>> _getRacesQueryHandler;
    private readonly IWalletService _walletService;

    public ClientsNotifier(IHubContext<RacesHub> hubContext, IQueryHandler<GetRacesQuery, List<RaceResponse>> getRacesQueryHandler, IWalletService walletService)
    {
        _hubContext = hubContext;
        _getRacesQueryHandler = getRacesQueryHandler;
        _walletService = walletService;
    }

    public async Task NotifyNewUpcomingRacesAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetRacesQuery();
        Result<List<RaceResponse>> result = await _getRacesQueryHandler.Handle(query, cancellationToken);
        if (result.IsSuccess)
        {
            await _hubContext.Clients.Group("RacesGroup").SendAsync("UpcomingRaces", result.Value, cancellationToken);
        }
    }

    public async Task NotifyRaceFinishedAsync(RaceFinishedMessage message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group("RacesGroup").SendAsync("RaceFinished", message, cancellationToken);
    }

    public async Task NotifyBalanceChangedAsync(CancellationToken cancellationToken = default)
    {
        decimal newBalance = await _walletService.GetBalanceAsync(Guid.NewGuid(), cancellationToken);
        await _hubContext.Clients.Group("RacesGroup").SendAsync("BalanceChange", newBalance, cancellationToken);
    }


}
