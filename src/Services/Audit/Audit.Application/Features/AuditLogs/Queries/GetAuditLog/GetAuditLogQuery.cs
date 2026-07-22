using BuildingBlock.Contract.Events.Audit;

namespace Audit.Application.Features.AuditLogs.Queries.GetAuditLog;

public sealed record GetAuditLogQuery(Guid AuditLogId) : IQuery<GetAuditLogResponse>;

/// <summary>Reuses the Contract-shaped AuditNode/AuditMetadata for the response too - symmetric with RecordAuditLogCommand's input shape, avoids a third parallel tree type just for reads.</summary>
public sealed record GetAuditLogResponse(
    Guid Id,
    string RootEntityType,
    string RootEntityId,
    string Service,
    string CorrelationId,
    AuditNode Root,
    AuditMetadata? Metadata,
    DateTime Timestamp,
    DateTime ReceivedAt);
