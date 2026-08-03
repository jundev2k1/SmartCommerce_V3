namespace SmartEcommerce.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User representing one tokenized payment method. Token/ExternalPaymentMethodId
/// are opaque references issued by the Provider's gateway - the real PAN/CVV never enters this
/// service, only what the gateway already considers safe to display (see CardInformation).
/// </summary>
public sealed class UserPaymentMethod : BaseEntity<Guid>, IAuditable
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public PaymentProvider Provider { get; private set; }
    public PaymentType PaymentType { get; private set; }
    public string? ExternalCustomerId { get; private set; }
    public string ExternalPaymentMethodId { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public CardInformation? CardInformation { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsVerified { get; private set; }

    private UserPaymentMethod() { }

    public static UserPaymentMethod Create(
        Guid userId,
        PaymentProvider provider,
        PaymentType paymentType,
        string externalPaymentMethodId,
        string token,
        string displayName,
        string? externalCustomerId = null,
        CardInformation? cardInformation = null,
        bool isDefault = false)
    {
        ValidateExternalPaymentMethodId(externalPaymentMethodId);
        ValidateToken(token);
        ValidateDisplayName(displayName);

        return new UserPaymentMethod
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Provider = provider,
            PaymentType = paymentType,
            ExternalCustomerId = externalCustomerId,
            ExternalPaymentMethodId = externalPaymentMethodId,
            Token = token,
            DisplayName = displayName,
            CardInformation = cardInformation,
            IsDefault = isDefault,
            IsVerified = false,
        };
    }

    // ============================================================================
    // Default flag
    // Manages the IsDefault toggle. The User aggregate root unmarks any previous
    // default before calling MarkAsDefault here, keeping the "at most one default
    // payment method" invariant centralized on User.
    // ============================================================================

    #region Default flag

    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Token refresh, display name, verification flag, and the shared
    // field-validation rules. Refreshing the token resets verification, since
    // the previously verified token no longer applies.
    // ============================================================================

    #region Details & lifecycle

    public void RefreshToken(string token, string externalPaymentMethodId)
    {
        ValidateToken(token);
        ValidateExternalPaymentMethodId(externalPaymentMethodId);

        Token = token;
        ExternalPaymentMethodId = externalPaymentMethodId;
        IsVerified = false;
    }

    public void Rename(string displayName)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
    }

    public void Verify()
    {
        IsVerified = true;
    }

    public void Unverify()
    {
        IsVerified = false;
    }

    private static void ValidateExternalPaymentMethodId(string externalPaymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentMethodId))
            throw ExceptionFactory.RequiredField("External payment method id cannot be empty.");
    }

    private static void ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw ExceptionFactory.RequiredField("Payment method token cannot be empty.");
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw ExceptionFactory.RequiredField("Payment method display name cannot be empty.");
    }

    #endregion
}
