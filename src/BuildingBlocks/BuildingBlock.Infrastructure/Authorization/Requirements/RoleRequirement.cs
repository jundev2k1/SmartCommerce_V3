using Microsoft.AspNetCore.Authorization;

namespace BuildingBlock.Infrastructure.Authorization.Requirements;

/// <summary>
/// Authorization requirement to check user role.
/// Used with RequireRole attribute in endpoints.
/// </summary>
public sealed class RoleRequirement(string role) : IAuthorizationRequirement
{
    public string Role { get; } = role;
}

/// <summary>
/// Authorization requirement to check user has any of the specified roles.
/// </summary>
public sealed class AnyRoleRequirement(params string[] roles) : IAuthorizationRequirement
{
    public IEnumerable<string> Roles { get; } = roles;
}
