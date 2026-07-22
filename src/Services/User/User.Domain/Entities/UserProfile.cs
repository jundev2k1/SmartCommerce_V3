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
    public string LastName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(
        Guid id,
        string email,
        string userName,
        string phoneNumber,
        string firstName,
        string lastName,
        UserStatus status = UserStatus.Active)
    {
        var user = new UserProfile
        {
            Id = id,
            Email = email,
            UserName = userName,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            Status = status,
        };
        user.SyncPhoneSearchFields();
        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        SyncPhoneSearchFields();
        Tourch();
    }

    private void SyncPhoneSearchFields()
    {
        PhoneSearch = PhoneNormalizer.Normalize(PhoneNumber);
        PhoneReverse = PhoneNormalizer.Reverse(PhoneSearch);
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        Tourch();
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        Tourch();
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        Tourch();
    }
}
