# Reference: Caching

**Scope:** `ICacheService`/Redis usage, the role-caching decorator pattern, and the Gateway's separate minimal Redis path. Supersedes/merges the old `CACHING.md` + `ROLE_CACHING.md` (archived, see [08-migration-plan.md](../08-migration-plan.md)).

## Standard path: `ICacheService`

`BuildingBlock.Application.Abstractions.Services.ICacheService` — generic Get/GetMany/Set/SetMany/Remove/RemoveMany/RemoveByPattern/Exists, all `<T>`, all `async`. Implementation: `BuildingBlock.Infrastructure.Caching.RedisCacheService` (JSON-serializes via `BuildingBlock.SharedKernel.Serialization.JsonSerializerConfiguration.Default`). Registered via `services.AddRedisCache(configuration)` (binds `"Cache"` config section → `CacheOptions`: `ConnectionString`, `DefaultExpirationMinutes`, `EnableCompression`, `KeyPrefix`, `ConnectionTimeout`).

**Note:** `CacheOptions.KeyPrefix` is bound from config but not actually applied anywhere in `RedisCacheService` — keys are exactly what you pass in, no automatic prefixing happens despite the option existing. Don't rely on it; build fully-qualified keys yourself via `BuildingBlock.SharedKernel.Constants.CacheKeys`.

### Key convention

All cache keys are centralized in `BuildingBlock.SharedKernel/Constants/CacheKeys.cs` as static builder methods, one nested class per cached entity type (`CacheKeys.Roles.UserRoles(userId)`, `CacheKeys.RefreshTokens.ByTokenString(token)`, etc.). **Add new key builders here, don't inline key strings in a service.** This is what lets a cross-cutting consumer (like the Gateway, below) share the exact key format with the service that owns the write side.

## Decorator pattern: caching an existing service

Don't cache inside a repository or handler ad hoc. Wrap the service:

```csharp
public sealed class Cached{X}ServiceDecorator(I{X}Service inner, {X}CacheService cache) : I{X}Service
{
    public async Task<Y> GetAsync(...) => await cache.GetAsync(...) ?? await CacheAndReturn(...);
    // delegate everything else to `inner`, invalidating cache on writes
}
```

Register manually (not Scrutor — the decorator needs the *concrete* inner implementation resolved explicitly):
```csharp
services.AddScoped<RoleCacheService>();
services.AddScoped<IAuthService>(sp =>
{
    var inner = sp.GetRequiredService<Auth.Persistence.Services.AuthService>();
    var cache = sp.GetRequiredService<RoleCacheService>();
    return new CachedAuthServiceDecorator(inner, cache);
});
```
Canonical example: `Auth.Infrastructure/Caching/{RoleCacheService,CachedAuthServiceDecorator}.cs`, registered in `Auth.Infrastructure/DependencyInjection.cs`'s `AddRoleCaching()`. TTL sourced from config (`Caching:EntityTtl:Roles:MinutesToExpire`) with a fallback constant (`CacheKeys.Roles.DefaultTtlMinutes`).

## Cross-service read-only consumer

A service can read a cache another service owns and writes, as long as it uses the same `CacheKeys` builder. Example: `User.Infrastructure/Caching/RoleCacheReader.cs` (`IRoleCacheReader`) reads `CacheKeys.Roles.UserRoles(userId)` — the same key Auth's `RoleCacheService` writes — without User owning any write path to it. If you add a reader like this, **never** write to a key another service owns; only the owning service's write path should mutate it.

## Gateway minimal lookup vs `ICacheService`

The API Gateway does **not** use `ICacheService`/`CacheOptions` at all. `BuildingBlock.Web/RefreshTokens/RefreshTokenCacheExtensions.cs` provides a standalone `AddRefreshTokenCache(connectionString)` (raw `IConnectionMultiplexer`, no serialization/options layer) + a single `RefreshTokenExistsAsync` extension method doing one `EXISTS` check via `CacheKeys.RefreshTokens.ByTokenString(token)`. This is deliberate — see [services/gateway.md](../services/gateway.md#refresh-token-filtering) and [decisions/buildingblock-web-extraction.md](../decisions/buildingblock-web-extraction.md). **Do not "fix" this by switching the Gateway to `ICacheService`** — that would pull `BuildingBlock.Infrastructure`'s full package set into the Gateway for a single `EXISTS` check, which is the opposite of what this design intentionally avoids.

## When to add caching

Only after measuring a real read-heavy, infrequently-changing path — see [workflows/performance-optimization.md](../workflows/performance-optimization.md). Don't cache write-heavy or per-request-unique data.
