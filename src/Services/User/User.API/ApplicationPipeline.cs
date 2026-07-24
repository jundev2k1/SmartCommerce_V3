using BuildingBlock.Web.Cors;
using BuildingBlock.Web.Middleware;
using BuildingBlock.Web.Swagger;

using User.API.ExceptionHandling;
using User.API.GrpcServices;
using User.Infrastructure.BackgroundJobs;
using User.Persistence;
using User.Persistence.Engine;
using User.Persistence.Seeders;

namespace User.API;

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
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var seeder = new UserSeeder(context);
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
        app.MapGrpcService<UserGrpcServiceImpl>();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }
}
