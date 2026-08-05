using System.Security.Claims;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Custom claim type keys used across the platform, plus the ClaimsPrincipal helpers built on
/// them. Named "App" to avoid colliding with System.Security.Claims.ClaimTypes. Add new claim
/// type keys here as they're introduced.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Claim type carrying one permission key per claim - see Permissions.</summary>
    public const string Permission = "permission";

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
        => principal.FindAll(Permission).Select(c => c.Value);

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
