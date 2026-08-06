namespace NovaCore.BuildingBlock.SharedKernel.Context;

/// <summary>
/// Immutable snapshot of the current request's identity, extracted once by
/// ExecutionContextMiddleware. Every field is either read straight off the JWT/headers or
/// generated when absent (CorrelationId) - nothing here is looked up lazily. Anonymous requests
/// (login, refresh token, health checks, public endpoints) simply carry null UserId/TenantId/ScopeId.
/// </summary>
public sealed class ExecutionContextData
{
    public Guid? UserId { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? ScopeId { get; init; }

    public required string CorrelationId { get; init; }

    public string? IdempotencyKey { get; init; }

    public string? Locale { get; init; }
}
