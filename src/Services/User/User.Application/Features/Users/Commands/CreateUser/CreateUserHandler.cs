using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.User;
using BuildingBlock.SharedKernel.Constants;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler(
    IUserProfileWriteService userWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    IAuthClientService authClient,
    ICurrentUserService currentUser) : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken ct = default)
    {
        // Check existing email
        var isExistEmail = await authClient.EmailExistsAsync(request.Email, ct);
        if (isExistEmail)
            throw new ConflictException("UserProfile {UserId} already exists, returning existing profile (idempotent)");

        // Check valid roles from input
        EnsureCallerMayGrantRoles(request.Roles);

        var correlationId = currentUser.GetCorrelationId();
        UserProfile user = null!;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            user = await userWriteService.CreateAsync(
                new CreateUserProfileRequest(
                    Guid.CreateVersion7(),
                    request.Email,
                    request.UserName,
                    request.PhoneNumber,
                    request.FirstName,
                    request.MiddleName,
                    request.LastName,
                    request.Roles),
                ct);
            await PublishProfileCreatedEventAsync(
                user,
                request.Roles,
                request.TempPassword,
                correlationId,
                ct);
        }, ct: ct);

        return new CreateUserResponse(user.Id);
    }

    private void EnsureCallerMayGrantRoles(string[] roles)
    {
        if (roles.Contains(AppRole.Root))
            throw new ForbiddenException("Cannot assign the Root role.");

        if (roles.Any(r => r != AppRole.User) && !currentUser.IsInRole(AppRole.Root))
            throw new ForbiddenException("Only Root can assign the Admin role.");
    }

    private async Task PublishProfileCreatedEventAsync(
        UserProfile createdUser,
        string[] roles,
        string tempPassword,
        string correlationId,
        CancellationToken ct)
    {
        var integrationEvent = new UserProfileCreatedIntegrationEvent(
            createdUser.Id,
            createdUser.Email,
            createdUser.UserName,
            createdUser.FirstName,
            createdUser.MiddleName,
            createdUser.LastName,
            correlationId,
            roles,
            tempPassword);
        await outboxStore.EnqueueAsync(integrationEvent, ct);
    }
}
