using System.Security.Claims;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.Infrastructure.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetUserIdSafe(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value
        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Email)?.Value;

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>
    /// Root holds every permission, so it satisfies any role check.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
        => principal.FindAll(ClaimTypes.Role)
            .Any(c => c.Value == AppRoleConstant.Root || c.Value == role);

    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
        => principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Any(r => r == AppRoleConstant.Root || roles.Contains(r, StringComparer.OrdinalIgnoreCase));

    public static bool HasAllRoles(this ClaimsPrincipal principal, params string[] roles)
        => roles.All(role => principal.HasRole(role));

    public static string? GetClaim(this ClaimsPrincipal principal, string claimType)
        => principal.FindFirst(claimType)?.Value;

    public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal principal, string claimType)
        => principal.FindAll(claimType).Select(c => c.Value);
}
