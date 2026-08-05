using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.DeadLetters.Commands;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SmartEcommerce.BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class RetryAllDeadLetters : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/retry-all", RetryAll)
            .WithTags("DeadLetter")
            .RequirePermissions(Permissions.System.MessagingRequeue)
            .WithName("RetryAllDeadLetters")
            .WithDisplayName("Retry All Dead Letters API")
            .WithDescription("Retries every DeadLetter row matching an optional filter, capped at 500 per call.")
            .Produces<ApiResponse<RetryDeadLettersSummary>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RetryAll(
        [FromBody] CriteriaRequest? filter, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryAllDeadLettersCommand(filter), ct);
        return Results.Ok(ApiResponse<RetryDeadLettersSummary>.Ok(response));
    }
}
