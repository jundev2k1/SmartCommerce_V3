using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Web.Cors;
using BuildingBlock.Web.Middleware;
using BuildingBlock.Web.Swagger;

using Order.API.ExceptionHandling;
using Order.Infrastructure.BackgroundJobs;

namespace Order.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
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

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequiredHeadersMiddleware>();
        app.UseIdempotency();
        return app;
    }
}
