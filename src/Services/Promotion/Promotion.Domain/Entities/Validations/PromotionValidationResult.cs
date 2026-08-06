namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>The outcome of running a PromotionValidationPolicy against something - related by PolicyId only, no evaluation logic lives here.</summary>
public sealed class PromotionValidationResult : BaseEntity<Guid>, IAuditable
{
    public Guid PolicyId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? Message { get; private set; }

    private PromotionValidationResult() { }

    public static PromotionValidationResult Create(Guid policyId, string status, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw ExceptionFactory.RequiredField("Result status cannot be empty.");

        return new PromotionValidationResult
        {
            Id = Guid.CreateVersion7(),
            PolicyId = policyId,
            Status = status,
            Message = message,
        };
    }
}
