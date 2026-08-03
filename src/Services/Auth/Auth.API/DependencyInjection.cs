using Auth.API.ExceptionHandling;

using BuildingBlock.Application.DeadLetters;
using BuildingBlock.Grpc.Server;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.Security.Jwt;
using BuildingBlock.Web;
using BuildingBlock.Web.Carter;
using BuildingBlock.Web.Cors;
using BuildingBlock.Web.CurrentUser;
using BuildingBlock.Web.HealthChecks;
using BuildingBlock.Web.Swagger;

namespace Auth.API;

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
