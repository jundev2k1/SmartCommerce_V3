using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using BuildingBlock.SharedKernel.Extensions;

using Audit.Application.Features.AuditLogs.Queries.GetAuditLog;

namespace Audit.API.Endpoints;

public sealed class GetAuditLogEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Audit Log Details",
        "",
        "Retrieves a single audit log entry by id, including its raw event payload. Admin only.",
        "",
        "### Route Parameters",
        "- **auditLogId**: Unique identifier of the audit log entry (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Audit log entry not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/audit-logs/{auditLogId}", Handle)
            .WithTags("Audit")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetAuditLog")
            .WithDisplayName("Get Audit Log API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetAuditLogResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid auditLogId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetAuditLogQuery(auditLogId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetAuditLogResponse>.Ok(response));
    }
}
