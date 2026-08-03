using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.DeadLetters.Commands;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.SharedKernel.Constants;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlock.Web.Endpoints.DeadLetters;

public sealed class RetryAllDeadLetters : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/retry-all", RetryAll)
            .WithTags("DeadLetter")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
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
