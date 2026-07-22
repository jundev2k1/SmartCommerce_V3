namespace BuildingBlock.Infrastructure.Authorization;

/// <summary>
/// Authorization policy names for use with [Authorize(Policy = "...")]
/// </summary>
public static class AuthorizationPolicies
{
    public const string RequireAuthenticated = "RequireAuthenticated";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireUser = "RequireUser";
}
