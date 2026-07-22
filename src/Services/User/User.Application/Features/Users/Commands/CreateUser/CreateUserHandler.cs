using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.User;
using BuildingBlock.SharedKernel.Constants;

using User.Application.Abstractions.Repositories;
using User.Application.Abstractions.Services;

namespace User.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository userRepo,
    IUnitOfWork uow,
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

        // Create user profile
        var user = UserProfile.Create(
            Guid.CreateVersion7(),
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim(),
            request.FirstName.Trim(),
            request.LastName.Trim());

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await uow.ExecuteTransactionAsync(async () =>
        {
            await userRepo.AddAsync(user, ct);

            // Enqueue the UserProfileCreatedIntegrationEvent onto the same transaction as the profile write
            await PublishProfileCreatedEventAsync(user, request.Roles, request.TempPassword.Trim(), correlationId, ct);
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
            createdUser.LastName,
            correlationId,
            roles,
            tempPassword);
        await outboxStore.EnqueueAsync(integrationEvent, ct);
    }
}
