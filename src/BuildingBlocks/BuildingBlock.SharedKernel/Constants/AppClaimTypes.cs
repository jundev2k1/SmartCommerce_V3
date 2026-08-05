namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Custom claim type keys used across the platform. Named "App" to avoid colliding with
/// System.Security.Claims.ClaimTypes. Add new claim type keys here as they're introduced - this
/// class defines keys only, nothing reads or evaluates them.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Claim type carrying one permission key per claim - see Permissions.</summary>
    public const string Permission = "permission";
}
