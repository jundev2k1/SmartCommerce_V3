using YarpApiGateway;
using YarpApiGateway.Middleware;
using YarpApiGateway.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGatewayServices(builder.Configuration)
    .AddHttpContextAccessor();

var app = builder.Build();

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
