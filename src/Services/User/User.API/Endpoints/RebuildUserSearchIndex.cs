using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;
using BuildingBlock.SharedKernel.Extensions;

using User.Application.Features.Users.Commands.RebuildUserSearchIndex;

namespace User.API.Endpoints;

/// <summary>PostgreSQL -&gt; Projection Builder -&gt; Bulk Index -&gt; Elasticsearch. See docs/reference/search.md.</summary>
public sealed class RebuildUserSearchIndexEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Rebuild User Search Index",
        "",
        "**Requires Admin or Root** (RequireAdmin authorization policy) - documented explicitly here",
        "from day one, unlike the equivalent Product endpoint which initially shipped without this",
        "note despite the code already enforcing it.",
        "",
        "Drops and recreates the Elasticsearch user-search index, then repopulates it entirely",
        "from PostgreSQL (the source of truth). Use after a schema change (e.g. a new MiddleName",
        "field) or to recover from an out-of-sync index.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/search/rebuild", Handle)
            .WithTags("User")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("RebuildUserSearchIndex")
            .WithDisplayName("Rebuild User Search Index API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RebuildUserSearchIndexResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new RebuildUserSearchIndexCommand(), ct);

        return Results.Ok(ApiResponse<RebuildUserSearchIndexResponse>.Ok(response));
    }
}
