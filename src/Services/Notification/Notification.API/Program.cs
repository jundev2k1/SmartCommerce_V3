using Microsoft.AspNetCore.Server.Kestrel.Core;

using BuildingBlock.Infrastructure.Observability;
using BuildingBlock.Messaging.Kafka.Tracing;
using BuildingBlock.Observability.Logging;
using BuildingBlock.Observability.Tracing;
using BuildingBlock.Persistence.Mongo.DependencyInjection;

using Serilog;

using Notification.API;
using Notification.Application;
using Notification.Infrastructure;
using Notification.Persistence;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "notification-api"));

builder.WebHost.ConfigureKestrel(options =>
{
    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });
});

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddOpenTelemetryObservability(builder.Configuration, "notification-api", tracing => tracing
        .AddMongoTracing()
        .AddKafkaMessagingTracing()
        .AddInfrastructureTracing());

var app = builder.Build();

app.UseRedisTracing();

// No migration step here - Mongo is schemaless. The "notifications" collection and its indexes
// are created once by scripts/mongodb/init-mongo.js when the mongo container first initializes;
// Outbox/Inbox collection indexes are created by NotificationMongoContext's constructor instead
// (see BuildingBlock.Persistence.Mongo's Outbox/Inbox EnsureXIndexes() extensions).

app.UseApplication();

app.Run();
