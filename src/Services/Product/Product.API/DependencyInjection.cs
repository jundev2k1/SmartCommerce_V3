using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.CurrentUser;
using BuildingBlock.Infrastructure.DeadLetters;
using BuildingBlock.Infrastructure.Security.Jwt;
using BuildingBlock.Web;
using BuildingBlock.Web.Carter;
using BuildingBlock.Web.Cors;
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
            // Dead-letter management endpoints/handlers are generic and live in
            // BuildingBlock.Infrastructure - scanned here (the composition root) alongside this
            // service's own assembly, rather than duplicated per service.
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IDeadLetterRetryService).Assembly))
            .AddDeadLetterManagement()
            .AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService))
            .AddHealthCheckServices()
            .AddCommonAuthorizationPolicies()
            .AddAuthorization(options => AuthorizationExtensions.ConfigureCommonPolicies(options));

        return services;
    }
}
