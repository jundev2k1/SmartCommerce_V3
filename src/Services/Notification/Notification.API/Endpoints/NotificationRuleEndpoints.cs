using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationRules.Commands.CreateNotificationRule;
using Notification.Application.Features.NotificationRules.Queries.GetNotificationRule;
using Notification.Application.Features.NotificationRules.Queries.ListNotificationRules;

namespace Notification.API.Endpoints;

public sealed class NotificationRuleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-rules", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateNotificationRule")
            .WithDisplayName("Create Notification Rule API")
            .WithDescription("Defines what notification actions to create when a given business event occurs (e.g. OrderCreated -> User Notification + Email + Telegram).")
            .Produces<ApiResponse<CreateNotificationRuleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-rules/{ruleId}", GetAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationRule")
            .WithDisplayName("Get Notification Rule API")
            .Produces<ApiResponse<GetNotificationRuleResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-rules", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationRules")
            .WithDisplayName("List Notification Rules API")
            .Produces<ApiResponse<PaginatedResult<NotificationRuleSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationRuleCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationRuleResponse>.Ok(response));
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid ruleId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationRuleQuery(ruleId), ct);
        return Results.Ok(ApiResponse<GetNotificationRuleResponse>.Ok(response));
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? eventType,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationRulesQuery(
            eventType,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationRuleSummaryResponse>>.Ok(response));
    }
}
