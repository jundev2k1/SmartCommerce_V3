using BuildingBlock.Application.DeadLetters;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.Security.Jwt;
using BuildingBlock.Web;
using BuildingBlock.Web.Carter;
using BuildingBlock.Web.Cors;
using BuildingBlock.Web.CurrentUser;
using BuildingBlock.Web.HealthChecks;
using BuildingBlock.Web.Swagger;

using Notification.API.ExceptionHandling;

namespace Notification.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "SimpleShop Notification Service",
        ServiceDescription = "Notification Service API",
        SwaggerRoutePrefix = "/api/notification",
        ContactUrl = "http://localhost:5108",
        SwaggerUiTitle = "Notification Service",
        IncludeJwtBearerSwaggerAuth = false,
        CorsPolicyName = "AllowAll"
    };

    internal static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddExceptionHandling()
            .AddCurrentUser()
            .AddJwtBearerAuthentication(configuration)
            .AddSwaggerDocumentation(WebOptions)
            .AddCorsPolicy(WebOptions.CorsPolicyName)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IDeadLetterRetryService).Assembly))
            .AddDeadLetterManagement()
            .AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService))
            .AddHealthCheckServices()
            .AddCommonAuthorizationPolicies()
            .AddAuthorization(AuthorizationExtensions.ConfigureCommonPolicies)
            .AddSignalR();

        return services;
    }
}
