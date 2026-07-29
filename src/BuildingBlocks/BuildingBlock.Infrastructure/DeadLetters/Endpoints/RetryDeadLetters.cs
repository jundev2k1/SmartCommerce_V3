using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.Infrastructure.DeadLetters.Commands;

using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlock.Infrastructure.DeadLetters.Endpoints;

public sealed class RetryDeadLetters : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/deadletters/retry", RetryMany)
            .WithTags("DeadLetter")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("RetryDeadLetters")
            .WithDisplayName("Retry Selected Dead Letters API")
            .WithDescription("Retries a caller-supplied set of dead-lettered messages.")
            .Produces<ApiResponse<RetryDeadLettersSummary>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RetryMany(
        [FromBody] RetryDeadLettersRequest request, [FromServices] ISender sender, CancellationToken ct = default)
    {
        var response = await sender.Send(new RetryDeadLettersCommand(request.Ids), ct);
        return Results.Ok(ApiResponse<RetryDeadLettersSummary>.Ok(response));
    }
}

public sealed record RetryDeadLettersRequest(IReadOnlyList<Guid> Ids);
