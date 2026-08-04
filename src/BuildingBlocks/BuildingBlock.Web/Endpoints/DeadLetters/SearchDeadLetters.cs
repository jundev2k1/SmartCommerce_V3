using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Abstractions.DeadLetters;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Infrastructure.DeadLetters.Queries;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SmartEcommerce.BuildingBlock.Web.Endpoints.DeadLetters;

/// <summary>
/// Generic dead-letter management API, identical across every service that owns an Inbox table
/// (Audit, Auth, Inventory, Notification, Order, Product, User) - mounted once per service via
/// services.AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService)).
/// Mirrors the SearchOrders/CompleteOrder Carter conventions exactly.
/// </summary>
public sealed class SearchDeadLetters : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/search", Search)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.Manage)
            .WithName("SearchDeadLetters")
            .WithDisplayName("Search Dead Letters API")
            .WithDescription("Paged/filtered/sorted list of dead-lettered Inbox messages.")
            .Produces<ApiResponse<PaginatedResult<DeadLetterListItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Search(
        [FromBody] CriteriaRequest request, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new SearchDeadLettersQuery(request), ct);
        return Results.Ok(ApiResponse<PaginatedResult<DeadLetterListItemResponse>>.Ok(response));
    }
}
