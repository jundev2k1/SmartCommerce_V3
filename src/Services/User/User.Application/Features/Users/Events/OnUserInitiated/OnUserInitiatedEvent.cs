using BuildingBlock.Application.Abstractions.Events;

namespace User.Application.Features.Users.Events.OnUserInitiated;

public sealed record OnUserInitiatedEvent(
    Guid AccountId,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string LastName,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
