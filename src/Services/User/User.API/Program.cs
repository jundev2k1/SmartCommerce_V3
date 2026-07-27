using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using BuildingBlock.Infrastructure.Observability;
using BuildingBlock.Messaging.Kafka.Tracing;
using BuildingBlock.Observability.Logging;
using BuildingBlock.Observability.Tracing;

using Serilog;

using User.API;
using User.Application;
using User.Infrastructure;
using User.Persistence;
using User.Persistence.Engine;
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
}

app.UseRedisTracing();
app.UseApplication();

app.Run();
