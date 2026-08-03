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

using SmartEcommerce.User.API.ExceptionHandling;

namespace SmartEcommerce.User.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "SimpleShop User Service",
        ServiceDescription = "User Management Service API",
        SwaggerRoutePrefix = "/api/user",
        ContactUrl = "http://localhost:5101",
        SwaggerUiTitle = "User Service",
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
            .AddCurrentLocale()
            .AddJwtBearerAuthentication(configuration)
            .AddSwaggerDocumentation(WebOptions)
            .AddCorsPolicy(WebOptions.CorsPolicyName)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IDeadLetterRetryService).Assembly))
            .AddDeadLetterManagement()
            .AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService))
            .AddHealthCheckServices()
            .AddGrpcServices()
            .AddCommonAuthorizationPolicies()
            .AddAuthorization(AuthorizationExtensions.ConfigureCommonPolicies);

        return services;
    }

    private static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpcServer();
        return services;
    }
}
