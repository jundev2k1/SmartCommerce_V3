using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

using Serilog;

using Product.API;
using Product.Application;
using Product.Application.Abstractions.Search;
using Product.Infrastructure;
using Product.Persistence;
using Product.Persistence.Elasticsearch;
using Product.Persistence.Engine;
var builder = WebApplication.CreateBuilder(args);

var seqUrl = builder.Configuration["Logging:Seq:Url"] ?? "http://seq:5341";
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Seq(seqUrl);
});

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
    .AddElasticsearchPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.MigrateAsync();

    var searchIndexer = scope.ServiceProvider.GetRequiredService<IProductSearchIndexer>();
    await searchIndexer.EnsureIndexAsync();
}

app.UseApplication();

app.Run();
