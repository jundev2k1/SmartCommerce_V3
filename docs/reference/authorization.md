# Reference: Authorization

**Scope:** role/claims-based authorization inside services. Merges and supersedes `src/BuildingBlocks/BuildingBlock.Infrastructure/Authorization/{README,EXAMPLES,IMPLEMENTATION_SUMMARY}.md`, which were misplaced in the source tree rather than `docs/` (see [08-migration-plan.md](../08-migration-plan.md)). For the Gateway's role in this flow, see [services/gateway.md](../services/gateway.md#what-the-gateway-does--and-deliberately-does-not-do") — the Gateway validates token *integrity* only; it does not extract or forward roles/claims on the service's behalf.

## Flow

1. Client sends a request with a JWT (Authorization header or `AccessToken` cookie).
2. **Gateway**: validates signature/expiry/issuer/audience only, checks `RequireAuth` per route, does **not** resolve roles or attach claims for the service — see [services/gateway.md](../services/gateway.md).
3. **Service**: independently validates the same JWT via its own `AddJwtBearerAuthentication` (wired through `AddBuildingBlockWeb`, see [03-building-blocks-reference.md](../03-building-blocks-reference.md#web)), populating `HttpContext.User` itself. No database lookup, no call back to Auth Service — the JWT's claims (embedded at issuance by `Auth.Infrastructure/Security/Jwt/JwtTokenGenerator.cs`) are the sole source of truth for roles at this point.
4. Endpoint code reads claims via `ClaimsPrincipalExtensions` or declares a required policy/role.

## Registering policies (per service)

```csharp
// {Service}.API/DependencyInjection.cs, inside AddPresentation
services
    .AddBuildingBlockWeb(configuration, WebOptions)
    .AddCommonAuthorizationPolicies()                                  // registers RoleAuthorizationHandler + AnyRoleAuthorizationHandler
    .AddAuthorization(options => AuthorizationExtensions.ConfigureCommonPolicies(options));  // registers the common policies below
```

## Common policies (`AuthorizationPolicies`, `BuildingBlock.Infrastructure.Authorization`)

| Policy | Requirement |
|---|---|
| `RequireAuthenticated` | Any authenticated user |
| `RequireAdmin` | Root or Admin role |
| `RequireUser` | Root, Admin, or User role |

`AppRole.Root` (`BuildingBlock.SharedKernel.Constants`) always satisfies any role check — it's the superuser bypass, applied consistently in `ClaimsPrincipalExtensions.HasRole/HasAnyRole/HasAllRoles`.

## Custom per-service policy

```csharp
services
    .AddCommonAuthorizationPolicies()
    .AddAuthorizationBuilder()
    .AddPolicy("ManageCatalog", policy => policy.RequireAuthenticatedUser().RequireRole(AppRole.Root, AppRole.Admin))
    .Services;
// usage: app.MapPost(...).RequireAuthorization("ManageCatalog")
```

## Reading claims in a handler/endpoint

```csharp
var userId = user.GetUserIdSafe();      // 'sub' or NameIdentifier claim
var email = user.GetEmail();
var roles = user.GetRoles();
if (user.HasRole(AppRole.Admin)) { /* ... */ }
if (user.HasAnyRole(AppRole.User, AppRole.Admin)) { /* ... */ }
```
Full method list: `ClaimsPrincipalExtensions` (`BuildingBlock.Infrastructure/Authorization/ClaimsPrincipalExtensions.cs`) — `GetUserId`, `GetUserIdSafe`, `GetEmail`, `GetRoles`, `HasRole`, `HasAnyRole`, `HasAllRoles`, `GetClaim`, `GetClaimValues`.

Inside a Command/Query handler (not an endpoint), prefer injecting `ICurrentUserService` (`BuildingBlock.Application.Abstractions.Services`) over threading a `ClaimsPrincipal` through — it's the same identity data, already available via DI, and works uniformly whether the handler runs in a request or elsewhere.

## Custom attributes (marker only — not `[Authorize]`)

`BuildingBlock.Infrastructure/Authorization/Attributes/AuthorizeAttribute.cs` defines `AuthorizeAttribute`, `AuthorizeRoleAttribute`, `AllowAnonymousAttribute` as plain marker attributes — they are **not** ASP.NET Core's `[Authorize]`/`[AllowAnonymous]` and have no framework enforcement wired to them today. Use `.RequireAuthorization(policyName)` / `.AllowAnonymous()` (Carter's endpoint-builder extension methods) for actual enforcement, as shown above.

## Important

- Don't re-authenticate credentials at the service level — trust the JWT's claims once signature/expiry validation passes.
- Don't call an external auth service to check roles — they're in the token.
- Define custom policies per service, not in a shared BuildingBlock — each service's authorization needs are independent.
