using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.DeadLetters;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.DeadLetters.Commands;
using BuildingBlock.Infrastructure.DeadLetters.Queries;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlock.Infrastructure.DeadLetters;

/// <summary>
/// Generic dead-letter management API, identical across every service that owns an Inbox table
/// (Audit, Auth, Inventory, Notification, Order, Product, User) - mounted once per service via
/// services.AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService)).
/// Mirrors the SearchOrders/CompleteOrder Carter conventions exactly.
/// </summary>
public sealed class DeadLetterEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/search", Search)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("SearchDeadLetters")
            .WithDisplayName("Search Dead Letters API")
            .WithDescription("Paged/filtered/sorted list of dead-lettered Inbox messages.")
            .Produces<ApiResponse<PaginatedResult<DeadLetterListItemResponse>>>(StatusCodes.Status200OK);

        app.MapGet("/deadletters/{id:guid}", GetById)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetDeadLetter")
            .WithDisplayName("Get Dead Letter API")
            .WithDescription("Full detail for one dead-lettered row, including its retry history.")
            .Produces<ApiResponse<DeadLetterDetailResponse>>(StatusCodes.Status200OK);

        app.MapPost("/deadletters/{id:guid}/retry", RetryOne)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("RetryDeadLetter")
            .WithDisplayName("Retry Dead Letter API")
            .WithDescription("Requeues one dead-lettered message and republishes it through the normal Kafka pipeline.")
            .Produces<ApiResponse<RetryDeadLetterResponse>>(StatusCodes.Status200OK);

        app.MapPost("/deadletters/retry", RetryMany)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("RetryDeadLetters")
            .WithDisplayName("Retry Selected Dead Letters API")
            .WithDescription("Retries a caller-supplied set of dead-lettered messages.")
            .Produces<ApiResponse<RetryDeadLettersSummary>>(StatusCodes.Status200OK);

        app.MapPost("/deadletters/retry-all", RetryAll)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("RetryAllDeadLetters")
            .WithDisplayName("Retry All Dead Letters API")
            .WithDescription("Retries every DeadLetter row matching an optional filter, capped at 500 per call.")
            .Produces<ApiResponse<RetryDeadLettersSummary>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Search(
        [FromBody] CriteriaRequest request, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new SearchDeadLettersQuery(request), ct);
        return Results.Ok(ApiResponse<PaginatedResult<DeadLetterListItemResponse>>.Ok(response));
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new GetDeadLetterQuery(id), ct);
        return Results.Ok(ApiResponse<DeadLetterDetailResponse>.Ok(response));
    }

    private static async Task<IResult> RetryOne(
        [FromRoute] Guid id, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryDeadLetterCommand(id), ct);
        return Results.Ok(ApiResponse<RetryDeadLetterResponse>.Ok(response));
    }

    private static async Task<IResult> RetryMany(
        [FromBody] RetryDeadLettersRequest request, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryDeadLettersCommand(request.Ids), ct);
        return Results.Ok(ApiResponse<RetryDeadLettersSummary>.Ok(response));
    }

    private static async Task<IResult> RetryAll(
        [FromBody] CriteriaRequest? filter, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryAllDeadLettersCommand(filter), ct);
        return Results.Ok(ApiResponse<RetryDeadLettersSummary>.Ok(response));
    }
}

public sealed record RetryDeadLettersRequest(IReadOnlyList<Guid> Ids);
