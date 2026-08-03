using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;

using Notification.Application.Features.NotificationGroups.Queries.ListNotificationGroups;

namespace Notification.API.Endpoints.NotificationGroup;

public sealed class ListGroups : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-groups", ListAsync)
            .WithTags("NotificationGroup")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationGroups")
            .WithDisplayName("List Notification Groups API")
            .Produces<ApiResponse<PaginatedResult<NotificationGroupSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationGroupsQuery(
            search,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationGroupSummaryResponse>>.Ok(response));
    }
}
