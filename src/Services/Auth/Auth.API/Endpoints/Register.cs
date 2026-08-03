using SmartEcommerce.Auth.Application.Features.Auth.Commands.Register;

using SmartEcommerce.BuildingBlock.Domain.Enums;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.Web.Swagger.EndpointHeader;

namespace SmartEcommerce.Auth.API.Endpoints;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string PhoneNumber, string MiddleName = "");

public sealed class Register : ICarterModule
{
    private readonly string[] API_DESC = [
        "## User Registration",
        "",
        "Creates a new user account with email and password.",
        "",
        "### Request Body",
        "- **Email**: User email address (required, must be unique)",
        "- **Password**: User password (required)",
        "- **FirstName**: User first name (required)",
        "- **MiddleName**: User middle name (optional)",
        "- **LastName**: User last name (required)",
        "- **PhoneNumber**: User phone number (required)",
        "",
        "### Response",
        "Sets HTTP-only cookies for access and refresh tokens. No tokens in response body.",
        "",
        "### Cookies Set",
        "- **AccessToken**: HTTP-only secure cookie (15 min expiry)",
        "- **RefreshToken**: HTTP-only secure cookie (7 days expiry)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or email already exists",
        "- **500**: Registration failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new RegisterCommand(
                request.Email.Trim(),
                request.Password.Trim(),
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.PhoneNumber.Trim(),
                request.MiddleName.Trim());
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok(MessageCode.Created);
        })
        .WithTags("Authentication")
        .AllowAnonymous()
        .Headers([
            new HeaderDefinition(HeaderKeys.CorrelationId, true)
        ])
        .WithSummary("Auth_Register")
        .WithDisplayName("Register API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<object>>();
    }
}
