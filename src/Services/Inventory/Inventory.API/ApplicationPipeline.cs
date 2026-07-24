using BuildingBlock.Web.Cors;
using BuildingBlock.Web.Middleware;
using BuildingBlock.Web.Swagger;

using Inventory.API.ExceptionHandling;
using Inventory.API.GrpcServices;
using Inventory.Infrastructure.BackgroundJobs;
using Inventory.Persistence;
using Inventory.Persistence.Engine;
using Inventory.Persistence.Seeders;

namespace Inventory.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.SeedDatabase();
        app.UseBackgroundJobsDashboard();
        app.UseBackgroundJobsScheduling();
        app.UseGlobalExceptionHandling();
        app.UseSwaggerDocumentation(DependencyInjection.WebOptions.SwaggerUiTitle);
        app.UseCorsPolicy(DependencyInjection.WebOptions.CorsAppliedPolicyName ?? DependencyInjection.WebOptions.CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        app.UseMiddlewares();

        return app;
    }

    private static void SeedDatabase(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var seeder = new InventorySeeder(context);
            seeder.SeedAsync().Wait();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding the database");
            if (app.Environment.IsDevelopment())
                throw;
        }
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapGrpcService<InventoryGrpcServiceImpl>();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }
}
