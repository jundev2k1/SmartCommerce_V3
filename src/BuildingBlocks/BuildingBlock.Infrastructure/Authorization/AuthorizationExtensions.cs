using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.BuildingBlock.Infrastructure.Authorization.Handlers;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

namespace SmartEcommerce.BuildingBlock.Infrastructure.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers authorization handlers and setup.
    /// Services should call AddAuthorization() in their own DependencyInjection
    /// after calling this method to define their specific policies.
    /// </summary>
    public static IServiceCollection AddCommonAuthorizationPolicies(
        this IServiceCollection services)
    {
        // Ensure AddAuthorization() is called in the service's Program.cs
        // This registers the handlers that our policies will use
        services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AnyRoleAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Helper to configure common authorization policies in the service.
    /// Call this in your service's DependencyInjection.cs AddPresentation method:
    ///
    /// services.AddAuthorization(options =>
    /// {
    ///     ConfigureCommonPolicies(options);
    /// });
    /// </summary>
    public static void ConfigureCommonPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(AuthorizationPolicies.RequireAuthenticated, policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole(AppRole.Root, AppRole.Admin));

        options.AddPolicy(AuthorizationPolicies.RequireUser, policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole(AppRole.Root, AppRole.Admin, AppRole.User));
    }

    /// <summary>
    /// Registers authorization policies and discovers custom policies marked with IAuthorizationPolicy.
    /// </summary>
    public static IServiceCollection AddCustomAuthorizationPolicies(
        this IServiceCollection services,
        Type markerType)
    {
        var assembly = markerType.Assembly;
        var policyTypes = assembly.GetTypes()
            .Where(t => typeof(IAuthorizationPolicy).IsAssignableFrom(t)
                && !t.IsInterface
                && !t.IsAbstract)
            .ToList();

        if (!policyTypes.Any())
            return services;

        foreach (var policyType in policyTypes)
        {
            var instance = Activator.CreateInstance(policyType);
            if (instance is not IAuthorizationPolicy policy)
                continue;

            // Register as singleton
            services.AddSingleton(policyType);
        }

        return services;
    }
}
