using BuildingBlock.Application.Abstractions.Common;

namespace Auth.Application.Abstractions.Services;

public interface IUserProfileService
{
    Task<GrpcServiceResult<UserProfileData>> CreateUserProfileAsync(
        Guid accountId,
        string email,
        string userName,
        string firstName,
        string middleName,
        string lastName,
        string phoneNumber,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed record UserProfileData(Guid UserId);
