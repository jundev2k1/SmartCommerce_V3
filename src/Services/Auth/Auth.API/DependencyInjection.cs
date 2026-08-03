using SmartEcommerce.Auth.API.ExceptionHandling;

using SmartEcommerce.BuildingBlock.Application.DeadLetters;
using SmartEcommerce.BuildingBlock.Grpc.Server;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Security.Jwt;
using SmartEcommerce.BuildingBlock.Web;
using SmartEcommerce.BuildingBlock.Web.Carter;
using SmartEcommerce.BuildingBlock.Web.Cors;
using SmartEcommerce.BuildingBlock.Web.CurrentUser;
using SmartEcommerce.BuildingBlock.Web.HealthChecks;
using SmartEcommerce.BuildingBlock.Web.Swagger;

namespace SmartEcommerce.Auth.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "SimpleShop Auth Service",
        ServiceDescription = "Authentication and Authorization Service API",
        SwaggerRoutePrefix = "/api/auth",
        ContactUrl = "http://localhost:5100",
        SwaggerUiTitle = "Auth Service",
        IncludeJwtBearerSwaggerAuth = true,
        CorsPolicyName = "AllowAll",
        CorsAppliedPolicyName = "AllowGateway"
    };

    internal static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
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
            .AddGrpcServer()
            .AddCommonAuthorizationPolicies()
            .AddAuthorization(AuthorizationExtensions.ConfigureCommonPolicies);

        return services;
    }
}
