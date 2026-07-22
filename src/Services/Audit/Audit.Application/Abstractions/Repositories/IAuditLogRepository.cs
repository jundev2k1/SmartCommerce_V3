namespace Audit.Application.Abstractions.Repositories;

/// <summary>
/// Audit logs are append-only (no update/delete), so this does not implement the generic
/// BuildingBlock.Persistence.Repository.IRepository&lt;T&gt; - same reasoning as Inventory's
/// IInventoryTransactionRepository. SearchAsync exists because a paged/filtered read is the
/// only real query shape this service needs; there is no per-entity CRUD beyond that.
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(AuditLogEntry entity, CancellationToken ct = default);

    Task<(IReadOnlyList<AuditLogEntry> Items, int TotalCount)> SearchAsync(
        string? service,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
