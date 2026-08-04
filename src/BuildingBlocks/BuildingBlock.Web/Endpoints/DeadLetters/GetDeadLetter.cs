using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Abstractions.DeadLetters;
using SmartEcommerce.BuildingBlock.Infrastructure.DeadLetters.Queries;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SmartEcommerce.BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class GetDeadLetter : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/deadletters/{id:guid}", GetById)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.Manage)
            .WithName("GetDeadLetter")
            .WithDisplayName("Get Dead Letter API")
            .WithDescription("Full detail for one dead-lettered row, including its retry history.")
            .Produces<ApiResponse<DeadLetterDetailResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new GetDeadLetterQuery(id), ct);
        return Results.Ok(ApiResponse<DeadLetterDetailResponse>.Ok(response));
    }
}
