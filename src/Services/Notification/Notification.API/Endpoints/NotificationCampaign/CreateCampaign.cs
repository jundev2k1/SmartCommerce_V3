using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationCampaigns.Commands.CreateNotificationCampaign;

namespace Notification.API.Endpoints.NotificationCampaign;

public sealed class CreateCampaign : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-campaigns", HandleAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateNotificationCampaign")
            .WithDisplayName("Create Notification Campaign API")
            .WithDescription("Creates a broadcast campaign (once or recurring) targeting a NotificationGroup audience. Starts in Draft - call Activate separately once execution is implemented.")
            .Produces<ApiResponse<CreateNotificationCampaignResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] CreateNotificationCampaignCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationCampaignResponse>.Ok(response));
    }
}
