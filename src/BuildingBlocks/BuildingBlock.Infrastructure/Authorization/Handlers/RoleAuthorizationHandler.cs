using Microsoft.AspNetCore.Authorization;

using BuildingBlock.Infrastructure.Authorization.Requirements;

namespace BuildingBlock.Infrastructure.Authorization.Handlers;

public sealed class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (context.User.HasRole(requirement.Role))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public sealed class AnyRoleAuthorizationHandler : AuthorizationHandler<AnyRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyRoleRequirement requirement)
    {
        if (context.User.HasAnyRole(requirement.Roles.ToArray()))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
