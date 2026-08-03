using Microsoft.AspNetCore.Server.Kestrel.Core;

using SmartEcommerce.BuildingBlock.Infrastructure.Observability;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Tracing;
using SmartEcommerce.BuildingBlock.Observability.Logging;
using SmartEcommerce.BuildingBlock.Observability.Tracing;
using SmartEcommerce.BuildingBlock.Persistence.Mongo.DependencyInjection;

using Serilog;

using SmartEcommerce.Notification.API;
using SmartEcommerce.Notification.Application;
using SmartEcommerce.Notification.Infrastructure;
using SmartEcommerce.Notification.Persistence;
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
// (see SmartEcommerce.BuildingBlock.Persistence.Mongo's Outbox/Inbox EnsureXIndexes() extensions).

app.UseApplication();

app.Run();
