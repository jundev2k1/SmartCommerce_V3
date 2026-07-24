using Auth.API;
using Auth.Application;
using Auth.Infrastructure;
using Auth.Persistence;
using Auth.Persistence.Engine;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using Serilog;
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with Seq
var seqUrl = builder.Configuration["Logging:Seq:Url"] ?? "http://seq:5341";
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Seq(seqUrl);
});

// Expose REST and gRPC on separate ports
builder.WebHost.ConfigureKestrel(options =>
{

    // Public REST API
    var httpPort = int.Parse(builder.Configuration["ASPNETCORE_HTTP_PORT"] ?? "8080");
    options.ListenAnyIP(httpPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http1;
    });

    // Internal gRPC
    var grpcPort = int.Parse(builder.Configuration["ASPNETCORE_GRPC_PORT"] ?? "5002");
    options.ListenAnyIP(grpcPort, listen =>
    {
        listen.Protocols = HttpProtocols.Http2;
    });
});

// Add services
builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure middleware pipeline
app.UseApplication();

app.Run();
