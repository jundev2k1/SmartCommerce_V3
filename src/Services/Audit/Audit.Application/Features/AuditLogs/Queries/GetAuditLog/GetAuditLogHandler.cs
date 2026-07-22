using Audit.Application.Abstractions.Repositories;

using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Audit;

namespace Audit.Application.Features.AuditLogs.Queries.GetAuditLog;

public sealed class GetAuditLogHandler(IAuditLogRepository auditLogRepo)
    : IQueryHandler<GetAuditLogQuery, GetAuditLogResponse>
{
    public async Task<GetAuditLogResponse> Handle(GetAuditLogQuery request, CancellationToken ct = default)
    {
        var entry = await auditLogRepo.GetByIdAsync(request.AuditLogId, ct)
            ?? throw new NotFoundException("AuditLog", request.AuditLogId);

        return new GetAuditLogResponse(
            entry.Id,
            entry.RootEntityType,
            entry.RootEntityId,
            entry.Service,
            entry.CorrelationId,
            MapNode(entry.Root),
            MapMetadata(entry.Metadata),
            entry.Timestamp,
            entry.ReceivedAt);
    }

    private static AuditNode MapNode(AuditTrailNode node)
    {
        return new AuditNode(
            node.NodeId,
            node.ParentNodeId,
            node.Depth,
            node.EntityType,
            node.EntityId,
            Enum.Parse<AuditAction>(node.Action),
            [.. node.Changes.Select(c => new AuditFieldChange(c.PropertyName, c.OldValue, c.NewValue))],
            [.. node.Children.Select(MapNode)]);
    }

    private static AuditMetadata? MapMetadata(AuditTrailMetadata? metadata)
    {
        if (metadata is null)
            return null;

        return new AuditMetadata
        {
            Actor = metadata.Actor,
            Service = metadata.Service,
            ClientIp = metadata.ClientIp,
            UserAgent = metadata.UserAgent,
            BusinessAction = metadata.BusinessAction,
            Reason = metadata.Reason,
            RequestPath = metadata.RequestPath,
            TraceId = metadata.TraceId,
        };
    }
}
