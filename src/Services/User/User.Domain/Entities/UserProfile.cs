using BuildingBlock.Domain.Abstractions;
using BuildingBlock.SharedKernel.Text;

using User.Domain.Enums;

namespace User.Domain.Entities;

public sealed class UserProfile : AggregateRoot<Guid>, IAuditable
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PhoneSearch { get; private set; } = string.Empty;
    public string PhoneReverse { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }

    /// <summary>
    /// Denormalized snapshot of the roles Auth assigned at account-creation time - the only point
    /// roles are ever set today (no role-change endpoint exists yet). Lets search filter by role
    /// without a per-row cache/gRPC fan-out to Auth. See docs/tasks/2026-07-22/Task4_search-users-endpoint-already-exists.md.
    /// </summary>
    public string[] Roles { get; private set; } = [];

    private UserProfile() { }

    public static UserProfile Create(
        Guid id,
        string email,
        string userName,
        string phoneNumber,
        string firstName,
        string middleName,
        string lastName,
        string[] roles,
        UserStatus status = UserStatus.Active)
    {
        var user = new UserProfile
        {
            Id = id,
            Email = email,
            UserName = userName,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Roles = roles,
            Status = status,
        };
        user.SyncPhoneSearchFields();
        return user;
    }

    public void UpdateProfile(string firstName, string middleName, string lastName, string phoneNumber)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        SyncPhoneSearchFields();
    }

    private void SyncPhoneSearchFields()
    {
        PhoneSearch = PhoneNormalizer.Normalize(PhoneNumber);
        PhoneReverse = PhoneNormalizer.Reverse(PhoneSearch);
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
    }
}
