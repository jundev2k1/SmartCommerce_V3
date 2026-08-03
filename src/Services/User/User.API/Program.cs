using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using SmartEcommerce.BuildingBlock.Infrastructure.Observability;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Tracing;
using SmartEcommerce.BuildingBlock.Observability.Logging;
using SmartEcommerce.BuildingBlock.Observability.Tracing;

using Serilog;

using SmartEcommerce.User.API;
using SmartEcommerce.User.Application;
using SmartEcommerce.User.Application.Abstractions.Search;
using SmartEcommerce.User.Infrastructure;
using SmartEcommerce.User.Persistence;
using SmartEcommerce.User.Persistence.Engine;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "user-api"));

builder.WebHost.ConfigureKestrel(options =>
{

    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });

    var grpcPort = int.Parse(builder.Configuration["ASPNETCORE_GRPC_PORT"] ?? "5002");
    options.ListenAnyIP(grpcPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "user-api", tracing => tracing
        .AddPersistenceTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.MigrateAsync();

    var searchIndexer = scope.ServiceProvider.GetRequiredService<IUserSearchIndexer>();
    try
    {
        await searchIndexer.EnsureIndexAsync();
    }
    catch (Exception ex)
    {
        // Elasticsearch is a read-model dependency, not a hard requirement to serve traffic -
        // don't let a transient ES outage/misconfiguration take down the whole API on boot.
        app.Logger.LogError(ex, "Failed to ensure the user search index exists. Search endpoints will be degraded until Elasticsearch connectivity is restored.");
    }
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
