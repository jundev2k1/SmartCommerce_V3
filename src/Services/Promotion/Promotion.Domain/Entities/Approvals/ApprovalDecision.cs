namespace NovaCore.Promotion.Domain.Entities.Approvals;

/// <summary>A reviewer's decision on an ApprovalStep - Decision is a plain string (no enum requested). Not navigated from ApprovalWorkflow, so construction is public.</summary>
public sealed class ApprovalDecision : BaseEntity<Guid>, IAuditable
{
    public Guid StepId { get; private set; }
    public string Decision { get; private set; } = string.Empty;
    public DateTime DecidedAt { get; private set; }

    private ApprovalDecision() { }

    public static ApprovalDecision Create(Guid stepId, string decision)
    {
        if (string.IsNullOrWhiteSpace(decision))
            throw ExceptionFactory.RequiredField("Decision cannot be empty.");

        return new ApprovalDecision
        {
            Id = Guid.CreateVersion7(),
            StepId = stepId,
            Decision = decision,
            DecidedAt = DateTime.UtcNow,
        };
    }
}
