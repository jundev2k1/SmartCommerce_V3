namespace NovaCore.BuildingBlock.SharedKernel.Context;

/// <summary>
/// Ambient accessor for the current request's identity. Not a DbContext, not an application
/// service, not a DI-resolved abstraction - it exists purely so that framework components which
/// cannot (or should not) take a DI dependency on HttpContext - EF interceptors, model-building
/// conventions, Mapster profiles, Outbox/Inbox, Audit - can still read who/what a unit of work is
/// running for. Consumers only ever read <see cref="Current"/>; the AsyncLocal storage underneath
/// is an implementation detail. Only ExecutionContextMiddleware is allowed to call
/// <see cref="Initialize"/> - see that type, and docs/reference/execution-context.md, for why.
/// </summary>
public static class ExecutionContext
{
    private static readonly AsyncLocal<ExecutionContextData?> Storage = new();

    private static readonly ExecutionContextData Anonymous = new() { CorrelationId = string.Empty };

    public static ExecutionContextData Current => Storage.Value ?? Anonymous;

    /// <summary>Framework-only initialization point - called exclusively by ExecutionContextMiddleware,
    /// once per request, before any other component in the pipeline runs. Never call this from
    /// application code, a handler, or a background job; there is deliberately no update/mutate
    /// method, since request identity is read-only for the rest of the request's lifetime.</summary>
    public static void Initialize(ExecutionContextData data) => Storage.Value = data;
}
