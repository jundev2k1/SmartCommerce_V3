using SmartEcommerce.BuildingBlock.Application.DeadLetters;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Security.Jwt;
using SmartEcommerce.BuildingBlock.Web;
using SmartEcommerce.BuildingBlock.Web.Carter;
using SmartEcommerce.BuildingBlock.Web.Cors;
using SmartEcommerce.BuildingBlock.Web.CurrentUser;
using SmartEcommerce.BuildingBlock.Web.HealthChecks;
using SmartEcommerce.BuildingBlock.Web.Swagger;

using SmartEcommerce.Order.API.ExceptionHandling;

namespace SmartEcommerce.Order.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "SimpleShop Order Service",
        ServiceDescription = "Order Management Service API",
        SwaggerRoutePrefix = "/api/order",
        ContactUrl = "http://localhost:5101",
        SwaggerUiTitle = "Order Service",
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
