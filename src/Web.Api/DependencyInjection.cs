using Application.Abstractions.Messaging;
using Application.Races.Create;
using Infrastructure;
using Web.Api.Infrastructure;
using Infrastructure.Messaging.Handlers;
using Application.Abstractions;
using Web.Api.Hubs;
using Application.Races.Finish;

namespace Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddMessagePublishers();
        services.AddMessageConsumers();

        // Register message handlers
        services.AddScoped<IMessageHandler<RaceCreatedMessage>, RaceCreatedMessageHandler>();
        services.AddScoped<IMessageHandler<RaceFinishedMessage>, RaceFinishedMessageHandler>();
        services.AddScoped<IClientsNotifier, ClientsNotifier>();

        return services;
    }
}
