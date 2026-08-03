namespace SmartEcommerce.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Authorization policy names for use with [Authorize(Policy = "...")]
/// </summary>
public static class AuthorizationPoliciesConstant
{
    public const string RequireAuthenticated = "RequireAuthenticated";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireUser = "RequireUser";
}
