using Microsoft.AspNetCore.Authorization;

using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

namespace SmartEcommerce.BuildingBlock.Infrastructure.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Helper to configure common authorization policies in the service.
    /// Call this in your service's DependencyInjection.cs AddPresentation method:
    ///
    /// services.AddAuthorization(options =>
    /// {
    ///     ConfigureCommonPolicies(options);
    /// });
    ///
    /// Business-capability endpoints should use RequirePermissions(...) instead of a named policy -
    /// these two policies remain for endpoints that only need "is the caller logged in" /
    /// "is the caller acting on their own resource", not a specific permission.
    /// </summary>
    public static void ConfigureCommonPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(AuthorizationPoliciesConstant.RequireAuthenticated, policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy(AuthorizationPoliciesConstant.RequireUser, policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole(AppRoleConstant.Root, AppRoleConstant.Admin, AppRoleConstant.User));
    }
}
