using BuildingBlock.Application.DeadLetters;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.Security.Jwt;
using BuildingBlock.Web;
using BuildingBlock.Web.Carter;
using BuildingBlock.Web.Cors;
using BuildingBlock.Web.CurrentUser;
using BuildingBlock.Web.HealthChecks;
using BuildingBlock.Web.Swagger;

using Product.API.ExceptionHandling;

namespace Product.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "SimpleShop Product Service",
        ServiceDescription = "Product Catalog Management Service API",
        SwaggerRoutePrefix = "/api/product",
        ContactUrl = "http://localhost:5101",
        SwaggerUiTitle = "Product Service",
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
            .AddAuthorization(AuthorizationExtensions.ConfigureCommonPolicies);

        return services;
    }
}
