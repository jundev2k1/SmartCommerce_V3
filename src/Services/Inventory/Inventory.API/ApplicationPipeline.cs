using SmartEcommerce.BuildingBlock.Web.Cors;
using SmartEcommerce.BuildingBlock.Web.Middleware;
using SmartEcommerce.BuildingBlock.Web.Swagger;

using SmartEcommerce.Inventory.API.ExceptionHandling;
using SmartEcommerce.Inventory.API.GrpcServices;
using SmartEcommerce.Inventory.Infrastructure.BackgroundJobs;
using SmartEcommerce.Inventory.Persistence.Engine;
using SmartEcommerce.Inventory.Persistence.Storage.Seeders;

namespace SmartEcommerce.Inventory.API;

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
