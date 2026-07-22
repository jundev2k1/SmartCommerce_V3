using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationGroups.Commands.CreateNotificationGroup;
using Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;
using Notification.Application.Features.NotificationGroups.Queries.ListNotificationGroups;

namespace Notification.API.Endpoints;

public sealed class NotificationGroupEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-groups", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateNotificationGroup")
            .WithDisplayName("Create Notification Group API")
            .WithDescription("Creates a target audience (role, specific users, segment, ...) that campaigns broadcast to.")
            .Produces<ApiResponse<CreateNotificationGroupResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-groups/{groupId}", GetAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationGroup")
            .WithDisplayName("Get Notification Group API")
            .Produces<ApiResponse<GetNotificationGroupResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-groups", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationGroups")
            .WithDisplayName("List Notification Groups API")
            .Produces<ApiResponse<PaginatedResult<NotificationGroupSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationGroupCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationGroupResponse>.Ok(response));
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid groupId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationGroupQuery(groupId), ct);
        return Results.Ok(ApiResponse<GetNotificationGroupResponse>.Ok(response));
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
