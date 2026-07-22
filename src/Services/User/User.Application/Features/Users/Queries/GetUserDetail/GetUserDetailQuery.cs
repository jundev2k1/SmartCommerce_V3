namespace User.Application.Features.Users.Queries.GetUserDetail;

public sealed record GetUserDetailQuery : IQuery<GetUserDetailResponse>;

public sealed record GetUserDetailResponse(
    Guid Id,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string LastName,
    UserStatus Status,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt);
