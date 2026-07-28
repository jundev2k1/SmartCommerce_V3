using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Contract.Protos.User;

using Grpc.Core;

using User.Application.Features.Users.Events.OnUserInitiated;

namespace User.API.GrpcServices;

public sealed class UserGrpcServiceImpl(IInternalEventDispatcher eventDispatcher) : UserGrpcService.UserGrpcServiceBase
{
    public override async Task<CreateUserProfileResponse> CreateUserProfile(
        CreateUserProfileRequest request,
        ServerCallContext context)
    {
        var @event = new OnUserInitiatedEvent(
            Guid.Parse(request.UserId),
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim(),
            request.FirstName.Trim(),
            request.MiddleName.Trim(),
            request.LastName.Trim(),
            CorrelationId: request.CorrelationId);
        await eventDispatcher.PublishAsync(@event, context.CancellationToken);

        return new CreateUserProfileResponse
        {
            Success = true,
            UserId = @event.AccountId.ToString(),
        };
    }
}
