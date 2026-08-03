namespace SmartEcommerce.BuildingBlock.Infrastructure.Authorization.Attributes;

/// <summary>
/// Marks an endpoint as requiring authentication.
/// API Gateway validates JWT; this just reads the claim.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuthorizeAttribute : Attribute
{
    public AuthorizeAttribute() { }

    public AuthorizeAttribute(string policy)
    {
        Policy = policy;
    }

    public string? Policy { get; set; }
}

/// <summary>
/// Marks an endpoint as requiring a specific role.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuthorizeRoleAttribute(params string[] roles) : Attribute
{
    public string[] Roles { get; } = roles;
}

/// <summary>
/// Marks an endpoint as allowing anonymous access.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AllowAnonymousAttribute : Attribute;
