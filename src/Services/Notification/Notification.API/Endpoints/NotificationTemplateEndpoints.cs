using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationTemplates.Commands.CreateNotificationTemplate;
using Notification.Application.Features.NotificationTemplates.Queries.GetNotificationTemplate;
using Notification.Application.Features.NotificationTemplates.Queries.ListNotificationTemplates;

namespace Notification.API.Endpoints;

public sealed class NotificationTemplateEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-templates", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateNotificationTemplate")
            .WithDisplayName("Create Notification Template API")
            .WithDescription("Creates a reusable, channel-scoped template selected by rules/campaigns.")
            .Produces<ApiResponse<CreateNotificationTemplateResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-templates/{templateId}", GetAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationTemplate")
            .WithDisplayName("Get Notification Template API")
            .Produces<ApiResponse<GetNotificationTemplateResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-templates", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationTemplates")
            .WithDisplayName("List Notification Templates API")
            .Produces<ApiResponse<PaginatedResult<NotificationTemplateSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationTemplateCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationTemplateResponse>.Ok(response));
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid templateId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationTemplateQuery(templateId), ct);
        return Results.Ok(ApiResponse<GetNotificationTemplateResponse>.Ok(response));
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] NotificationChannelType? channel,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationTemplatesQuery(
            channel,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationTemplateSummaryResponse>>.Ok(response));
    }
}
