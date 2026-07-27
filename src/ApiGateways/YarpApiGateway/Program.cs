using BuildingBlock.Infrastructure.Observability;
using BuildingBlock.Observability.Logging;
using BuildingBlock.Observability.Tracing;

using Serilog;

using YarpApiGateway;
using YarpApiGateway.Middleware;
using YarpApiGateway.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "yarp-api-gateway"));

builder.Services
    .AddGatewayServices(builder.Configuration)
    .AddHttpContextAccessor()
    .AddOpenTelemetryObservability(builder.Configuration, "yarp-api-gateway", tracing => tracing
        .AddInfrastructureTracing());

var app = builder.Build();

app.UseRedisTracing();
app.MapHealthChecks("/health");
app.UseCorrelationId();
app.UseAuthentication();
app.UseRefreshTokenFilter();
app.UseGatewayAuthorization();

var swaggerAggregator = app.Services.GetRequiredService<ISwaggerAggregator>();

app.MapGet("/swagger", swaggerAggregator.ServeSwaggerIndexAsync);

app.MapReverseProxy(pipeline =>
{
    pipeline.UseSessionAffinity();
    pipeline.UseLoadBalancing();
});

app.Run();
