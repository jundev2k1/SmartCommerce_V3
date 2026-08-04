using System.Security.Claims;

using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

namespace SmartEcommerce.BuildingBlock.Application.Authorization;

public static class PermissionClaimsExtensions
{
    /// <summary>Claim type carrying one permission key per claim - see Permissions.</summary>
    public const string PermissionClaimType = "permission";

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
        => principal.FindAll(PermissionClaimType).Select(c => c.Value);

    /// <summary>
    /// Centralized permission resolution: Root bypasses everything, then each required permission
    /// is checked exact-match or via its module's aggregate "{module}:full" key. This is the only
    /// place that logic lives - RequirePermissions() on endpoints just calls this.
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        var owned = principal.GetPermissions().ToHashSet(StringComparer.Ordinal);

        if (owned.Contains(Permissions.Root))
            return true;

        foreach (var required in permissions)
        {
            if (owned.Contains(required))
                return true;

            var separatorIndex = required.IndexOf(':');
            if (separatorIndex > 0)
            {
                var aggregate = $"{required[..separatorIndex]}:full";
                if (owned.Contains(aggregate))
                    return true;
            }
        }

        return false;
    }
}
