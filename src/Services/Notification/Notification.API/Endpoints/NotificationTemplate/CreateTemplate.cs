using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationTemplates.Commands.CreateNotificationTemplate;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationTemplate;

public sealed class CreateTemplate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-templates", CreateAsync)
            .WithTags("NotificationTemplate")
            .RequirePermissions(Permissions.Notification.Manage)
            .WithName("CreateNotificationTemplate")
            .WithDisplayName("Create Notification Template API")
            .WithDescription("Creates a reusable, channel-scoped template selected by rules/campaigns.")
            .Produces<ApiResponse<CreateNotificationTemplateResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationTemplateCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationTemplateResponse>.Ok(response));
    }
}
