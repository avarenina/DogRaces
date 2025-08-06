using Microsoft.AspNetCore.Builder;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> database = builder
    .AddPostgres("database")
    .WithImage("postgres:17")
    .WithBindMount("../../.containers/db", "/var/lib/postgresql/data")
    .AddDatabase("dog-races");

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<ProjectResource> webApi = builder.AddProject<Projects.Web_Api>("web-api")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithEnvironment("ConnectionStrings__Redis", cache)
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database)
    .WaitFor(cache);

builder.AddProject<Projects.BackgroundService>("background-service")
    .WithEnvironment("ConnectionStrings__Database", database)
    .WithEnvironment("ConnectionStrings__Redis", cache)
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(webApi);


builder.Build().Run();
