using System.Text;
using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Infrastructure.Cache;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Messaging;
using Infrastructure.RNG;
using Infrastructure.Time;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddRedis(configuration)
            .AddHealthChecks(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddTransient<IRandomDoubleProvider, SecureRandomDoubleProvider>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
    private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is missing or empty in configuration.");
        }

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = redisConnectionString;
        });

        // Register a singleton Redis multiplexer
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // Register Medallion's distributed lock provider using the same multiplexer
        services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            IConnectionMultiplexer multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDistributedSynchronizationProvider(multiplexer.GetDatabase());
        });

        services.AddSingleton<IDistributedLockService, DistributedLockService>();

        return services;
    }

    public static IServiceCollection AddMessagePublishers(this IServiceCollection services)
    {
        services.AddSingleton<IMessagePublisher, RedisMessagePublisher>();
        return services;
    }

    public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
    {
        services.AddHostedService<RedisSubscriberService>();
        return services;
    }


    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!)
            .AddRedis(configuration.GetConnectionString("Redis")!);

        return services;
    }
}
