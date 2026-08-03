using SmartEcommerce.BuildingBlock.Web.Cors;
using SmartEcommerce.BuildingBlock.Web.Middleware;
using SmartEcommerce.BuildingBlock.Web.Swagger;

using SmartEcommerce.Notification.API.ExceptionHandling;
using SmartEcommerce.Notification.Infrastructure.BackgroundJobs;
using SmartEcommerce.Notification.Infrastructure.SignalR.Hubs.Global;

namespace SmartEcommerce.Notification.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseBackgroundJobsDashboard();
        app.UseBackgroundJobsScheduling();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseGlobalExceptionHandling()
            .UseSwaggerDocumentation(DependencyInjection.WebOptions.SwaggerUiTitle)
            .UseCorsPolicy(DependencyInjection.WebOptions.CorsAppliedPolicyName ?? DependencyInjection.WebOptions.CorsPolicyName)
            .MapEndpoints()
            .UseMiddlewares()
            .MapHubs();

        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }

    private static WebApplication MapHubs(this WebApplication app)
    {
        app.MapHub<GlobalHub>(GlobalHub.Path);
        return app;
    }
}
