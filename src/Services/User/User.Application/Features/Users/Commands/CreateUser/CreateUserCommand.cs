namespace User.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string[] Roles,
    string TempPassword = "") : ICommand<CreateUserResponse>;

public sealed record CreateUserResponse(Guid UserId);
