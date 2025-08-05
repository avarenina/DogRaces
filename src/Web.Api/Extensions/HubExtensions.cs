using Application.Abstractions.Messaging;
using Application.Races.Get;
using Microsoft.AspNetCore.SignalR;
using Web.Api.Hubs;

namespace Web.Api.Extensions;

public static class HubExtensions
{
    public static void MapHubs(this WebApplication app)
    {
        app.MapHub<RacesHub>("/racesHub");
    }
} 