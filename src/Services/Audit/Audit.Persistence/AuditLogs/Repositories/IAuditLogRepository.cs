namespace Audit.Persistence.AuditLogs.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entity, CancellationToken ct = default);
}
