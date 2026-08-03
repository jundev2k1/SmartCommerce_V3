using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;

using Notification.Application.Features.NotificationRules.Queries.GetNotificationRule;

namespace Notification.API.Endpoints.NotificationRule;

public sealed class GetRule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-rules/{ruleId}", GetAsync)
            .WithTags("NotificationRule")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationRule")
            .WithDisplayName("Get Notification Rule API")
            .Produces<ApiResponse<GetNotificationRuleResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid ruleId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationRuleQuery(ruleId), ct);
        return Results.Ok(ApiResponse<GetNotificationRuleResponse>.Ok(response));
    }
}
