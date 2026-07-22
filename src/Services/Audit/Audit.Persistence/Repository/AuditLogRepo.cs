using Audit.Application.Abstractions.Repositories;

namespace Audit.Persistence.Repository;

public sealed class AuditLogRepo(AuditMongoContext context) : IAuditLogRepository
{
    public async Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.AuditLogs.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(AuditLogEntry entity, CancellationToken ct = default)
    {
        await context.AuditLogs.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task<(IReadOnlyList<AuditLogEntry> Items, int TotalCount)> SearchAsync(
        string? service,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<AuditLogEntry>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(service))
            filter &= filterBuilder.Eq(x => x.Service, service);

        if (from is not null)
            filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

        if (to is not null)
            filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

        var totalCount = (int)await context.AuditLogs.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.AuditLogs
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
