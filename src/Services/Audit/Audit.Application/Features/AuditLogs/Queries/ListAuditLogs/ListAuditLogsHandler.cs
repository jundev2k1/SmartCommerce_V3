using BuildingBlock.Application.Abstractions.Common;

using Audit.Application.Abstractions.Repositories;

using Mapster;

namespace Audit.Application.Features.AuditLogs.Queries.ListAuditLogs;

public sealed class ListAuditLogsHandler(IAuditLogRepository auditLogRepo)
    : IQueryHandler<ListAuditLogsQuery, PaginatedResult<AuditLogSummaryResponse>>
{
    public async Task<PaginatedResult<AuditLogSummaryResponse>> Handle(ListAuditLogsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await auditLogRepo.SearchAsync(
            request.Service, request.From, request.To, request.Page, request.PageSize, ct);

        var mapped = items.Adapt<List<AuditLogSummaryResponse>>();

        return PaginatedResult<AuditLogSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
